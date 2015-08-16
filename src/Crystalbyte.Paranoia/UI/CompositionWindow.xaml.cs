#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CompositionWindow.xaml
    /// </summary>
    public partial class CompositionWindow : IAccentAware, ICompositionView {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MailCompositionContext _mailComposition;

        #endregion

        #region Construction

        public CompositionWindow()
            : this(new MailComposition()) {
        }

        internal CompositionWindow(MailComposition composition) {
            InitializeComponent();
            DataContext = _mailComposition = new MailCompositionContext(composition, this);
            HtmlEditor.EditorReady += async (sender, e) => await SignAsync();
        }

        #endregion

        #region Methods

        private void OnAttachmentMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            try {
                var item = (ListViewItem)sender;
                var attachment = (FileAttachmentContext)item.DataContext;
                attachment.Open();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnAttachmentsDelete(object sender, ExecutedRoutedEventArgs e) {
            try {
                var composition = (MailCompositionContext)DataContext;
                var listView = (ListView)sender;
                foreach (var item in listView.SelectedItems.OfType<FileAttachmentContext>().ToArray()) {
                    composition.Attachments.Remove(item);
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            Application.Current.AssertBackgroundThread();
            try {
                var contacts = QueryContacts(e.Text);
                e.Source = contacts;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal void CloseOverlay() {
            ModalOverlay.Visibility = Visibility.Collapsed;
            PopupFrame.Content = null;
        }

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            try {
                CloseOverlay();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnAccountComboboxDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                var context = (MailCompositionContext)DataContext;
                AccountComboBox.SelectedValue = context.Accounts.OrderByDescending(x => x.IsDefaultTime).FirstOrDefault();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnLink(object sender, ExecutedRoutedEventArgs e) {
            try {
                ModalOverlay.Visibility = Visibility.Visible;

                NavigationArguments.Push(HtmlEditor);
                var uri = typeof(InsertLinkModalPage).ToPageUri();
                PopupFrame.Navigate(uri);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnAttachment(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (MailCompositionContext)DataContext;
                context.InsertAttachments();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public void StartSendingAnimation() {
            var storyboard = (Storyboard)Resources["FlyOutStoryboard"];
            storyboard.Begin();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e) {
            try {
                Close();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public MailContactContext[] QueryContacts(string text) {
            Application.Current.AssertBackgroundThread();

            using (var database = new DatabaseContext()) {
                var candidates = database.MailContacts
                    .Where(x => x.Address.StartsWith(text)
                                || x.Name.StartsWith(text))
                    .Take(20)
                    .ToArray();

                return candidates.Select(x => new MailContactContext(x)).ToArray();
            }
        }

        internal void PrepareAsNew(IEnumerable<string> addresses) {
            HtmlEditor.Source = "message:///new";
            Loaded += OnLoadedAsNew;
        }

        private async void OnLoadedAsNew(object sender, RoutedEventArgs e) {
            try {
                Loaded -= OnLoadedAsNew;
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    RecipientsBox.Focus();
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal async Task PrepareAsReplyAsync(IReadOnlyDictionary<string, string> arguments) {
            var id = Int64.Parse(arguments["id"]);

            var info = await Task.Run(async () => {
                using (var database = new DatabaseContext()) {
                    return await database.MailMessages
                        .Where(x => x.Id == id)
                        .Select(x => new { x.Subject, x.Addresses })
                        .FirstOrDefaultAsync();
                }
            });

            if (info == null) {
                throw new InvalidOperationException();
            }

            HtmlEditor.EditorReady += OnContentReady;
            HtmlEditor.Source = string.Format("message:///reply?id={0}", id);

            var context = (MailCompositionContext)DataContext;
            context.Subject = string.Format("{0} {1}", Settings.Default.PrefixForAnswering, info.Subject);

            var from = info.Addresses.FirstOrDefault(x => x.Role == AddressRole.From);
            if (from != null) {
                RecipientsBox.Preset(new[] { from.Address });
            }
        }

        private async void OnSendToOutbox(object sender, ExecutedRoutedEventArgs e) {
            try {
                await _mailComposition.SaveToOutboxAsync();
                Close();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnSaveAsDraft(object sender, ExecutedRoutedEventArgs e) {
            try {

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnContentReady(object sender, EventArgs e) {
            try {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    HtmlEditor.Ready -= OnContentReady;
                    HtmlEditor.FocusEditor();
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal async Task PrepareAsReplyAllAsync(IReadOnlyDictionary<string, string> arguments) {
            var id = Int64.Parse(arguments["id"]);

            var info = await Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.Set<MailMessage>()
                        .Where(x => x.Id == id)
                        .Select(x => new { x.Subject, x.Addresses })
                        .FirstOrDefaultAsync();
                }
            });

            if (info == null) {
                throw new InvalidOperationException();
            }

            var fromAddresses = info.Addresses.Where(x => x.Role == AddressRole.From).Select(x => x.Address).ToArray();
            var from = Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.Set<MailContact>()
                        .Where(x => fromAddresses.Contains(x.Address))
                        .ToArrayAsync();
                }
            });

            var toAddresses = info.Addresses.Where(x => x.Role == AddressRole.To).Select(x => x.Address).ToArray();
            var to = Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.Set<MailContact>()
                        .Where(x => toAddresses.Contains(x.Address))
                        .ToArrayAsync();
                }
            });

            var ccAddresses = info.Addresses.Where(x => x.Role == AddressRole.Cc).Select(x => x.Address).ToArray();
            var cc = Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.Set<MailContact>()
                        .Where(x => ccAddresses.Contains(x.Address))
                        .ToArrayAsync();
                }
            });

            RecipientsBox.Preset((await from).Select(x => new MailContactContext(x)));
            RecipientsBox.Preset((await to).Select(x => new MailContactContext(x)));
            RecipientsBox.Preset((await cc).Select(x => new MailContactContext(x)));

            HtmlEditor.EditorReady += OnContentReady;
            HtmlEditor.Source = string.Format("message:///reply?id={0}", id);

            var c = (MailCompositionContext)DataContext;
            c.Subject = string.Format("{0} {1}", Settings.Default.PrefixForAnswering, info.Subject);
        }

        internal async Task PrepareAsForwardAsync(IReadOnlyDictionary<string, string> arguments) {
            var id = Int64.Parse(arguments["id"]);
            var info = await Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.Set<MailMessage>()
                        .Where(x => x.Id == id)
                        .Select(x => new { x.Subject, x.Addresses })
                        .FirstOrDefaultAsync();
                }
            });

            Loaded += OnLoadedAsNew;
            HtmlEditor.Source = string.Format("message:///forward?id={0}", id);
            var c = (MailCompositionContext)DataContext;
            c.Subject = string.Format("{0} {1}", Settings.Default.PrefixForForwarding, info.Subject);
        }

        private void OnHtmlSurfaceDrop(object sender, DragEventArgs e) {
            throw new NotImplementedException();
            //var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            //var context = DataContext as MailCompositionContext;
            //if (files == null | context == null)
            //    return;

            //files.ToList().ForEach(x => context.Attachments.Add(new FileBindableAttachmentContext(x)));
        }

        public static Window GetParentWindow(DependencyObject child) {
            while (true) {
                var parentObject = VisualTreeHelper.GetParent(child);

                if (parentObject == null) {
                    return null;
                }

                var parent = parentObject as Window;
                if (parent != null) return parent;
                child = parentObject;
            }
        }

        private async Task SignAsync() {
            var context = (MailCompositionContext)DataContext;
            var path = context.SelectedAccount.SignaturePath;

            string signature;
            if (!File.Exists(path)) {
                signature = string.Empty;
                var warning = string.Format(Paranoia.Properties.Resources.MissingSignatureTemplate, path);
                Logger.Warn(warning);
            } else {
                signature = await Task.Run(() => File.ReadAllText(path, Encoding.UTF8));
            }

            var bytes = Encoding.UTF8.GetBytes(signature);
            var encoded = Convert.ToBase64String(bytes);
            HtmlEditor.InsertSignature(encoded);
        }

        private async void OnAccountSelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                if (!HtmlEditor.IsLoaded) {
                    return;
                }

                var context = (MailCompositionContext)DataContext;
                if (context.SelectedAccount != null) {
                    await SignAsync();
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Implementation of OnAccentChanged

        public void OnAccentChanged() {
            BorderBrush = Application.Current.Resources[ThemeResourceKeys.AppAccentBrushKey] as Brush;
        }

        #endregion

        #region Implementation of ICompositionView

        public async Task<string> GetDocumentAsync() {
            var content = await HtmlEditor.GetHtmlAsync();
            return string.Format("<div>{0}</div>", content);
        }

        void IRecipientView.SetAddresses(MailCompositionAddress[] addresses) {
            RecipientsBox.Preset(addresses.Where(x => x.Role == AddressRole.To).ToArray());
            CarbonCopyBox.Preset(addresses.Where(x => x.Role == AddressRole.Cc).ToArray());
            BlindCarbonCopyBox.Preset(addresses.Where(x => x.Role == AddressRole.Bcc).ToArray());
        }

        IEnumerable<MailCompositionAddress> IRecipientView.GetAddresses() {
            var from = RecipientsBox.SelectedValues.Select(x => {
                var contact = x as MailContactContext;
                return contact != null
                    ? new MailCompositionAddress { Address = contact.Address, Role = AddressRole.To }
                    : new MailCompositionAddress { Address = x as string, Role = AddressRole.To };
            });

            var cc = CarbonCopyBox.SelectedValues.Select(x => {
                var contact = x as MailContactContext;
                return contact != null
                    ? new MailCompositionAddress { Address = contact.Address, Role = AddressRole.Cc }
                    : new MailCompositionAddress { Address = x as string, Role = AddressRole.Cc };
            });

            var bcc = BlindCarbonCopyBox.SelectedValues.Select(x => {
                var contact = x as MailContactContext;
                return contact != null
                    ? new MailCompositionAddress { Address = contact.Address, Role = AddressRole.Bcc }
                    : new MailCompositionAddress { Address = x as string, Role = AddressRole.Bcc };
            });

            return from.Concat(cc).Concat(bcc);
        }

        #endregion

        #region Implementation of IDocumentView

        void IDocumentView.Preset(MailComposition composition) {
            HtmlEditor.Source = string.Format("composition:///{0}", composition.Id);
        }

        #endregion

        private bool CanSend() {
            return RecipientsBox.SelectedValues.Length > 0;
        }

        private void OnCarbonCopyBoxSelectedValuesChanged(object sender, EventArgs e) {
            try {
                // Nada yet ...
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnBlindCarbonCopyBoxSelectedValuesChanged(object sender, EventArgs e) {
            try {
                // Nada yet ...
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnRecipientBoxSelectedValuesChanged(object sender, EventArgs e) {
            try {
                SendButton.IsEnabled = CanSend();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnSubjectTextChanged(object sender, TextChangedEventArgs e) {
            try {
                SendButton.IsEnabled = CanSend();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }
    }
}