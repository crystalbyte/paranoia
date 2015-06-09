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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Crystalbyte.Paranoia.Themes;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for InspectionWindow.xaml
    /// </summary>
    public partial class InspectionWindow : IAccentAware {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public InspectionWindow(MailMessageContext message) {
            InitializeComponent();
            DataContext = message;
            message.IsExternalContentAllowedChanged += OnIsExternalContentAllowedChanged;
            message.DownloadCompleted += OnDownloadCompleted;
            Loaded += (sender, e) => ViewMessage(message);
        }

        public InspectionWindow(FileMessageContext file) {
            InitializeComponent();
            DataContext = file;
            Loaded += (sender, e) => {
                HtmlViewer.Source = string.Format("file:///local?path={0}", Uri.EscapeDataString(file.FullName));
            };
        }

        #endregion

        #region Class Overrides

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            var message = DataContext as MailMessageContext;
            if (message == null)
                return;

            // Remove handler to cut the reference from the message to this window.
            message.DownloadCompleted -= OnDownloadCompleted;
            message.IsExternalContentAllowedChanged -= OnIsExternalContentAllowedChanged;
        }

        #endregion

        #region Methods

        private void OnDownloadCompleted(object sender, EventArgs e) {
            try {
                var message = DataContext as MailMessageContext;
                if (message != null) {
                    ViewMessage(message);
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            try {
                if (!IsLoaded) {
                    return;
                }
                var view = (ListView)sender;
                var attachment = (MailAttachmentContext)view.SelectedValue;
                if (attachment == null) {
                    return;
                }
                attachment.Open();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            try {
                await HtmlViewer.PrintAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnForward(object sender, ExecutedRoutedEventArgs e) {
            try {
                var file = DataContext as FileMessageContext;
                if (file != null) {
                    await App.Context.ForwardAsync(file);
                    return;
                }

                var message = DataContext as MailMessageContext;
                if (message == null)
                    return;

                await App.Context.ForwardAsync(message);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnReply(object sender, ExecutedRoutedEventArgs e) {
            try {
                var file = DataContext as FileMessageContext;
                if (file != null) {
                    await App.Context.ReplyAsync(file);
                    return;
                }

                var message = DataContext as MailMessageContext;
                if (message == null)
                    return;

                await App.Context.ReplyAsync(message);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                var file = DataContext as FileMessageContext;
                if (file != null) {
                    await App.Context.ReplyToAllAsync(file);
                    return;
                }

                var message = DataContext as MailMessageContext;
                if (message == null)
                    return;

                await App.Context.ReplyToAllAsync(message);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnShowToAddresses(object sender, ExecutedRoutedEventArgs e) {
            try {
                ToPopup.IsOpen = true;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnShowCcAddresses(object sender, ExecutedRoutedEventArgs e) {
            try {
                CcPopup.IsOpen = true;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void ViewMessage(MailMessageContext message) {
            HtmlViewer.Source = string.Format(message.IsExternalContentAllowed
               ? "message:///{0}?blockExternals=false"
               : "message:///{0}", message.Id);
        }

        private void OnIsExternalContentAllowedChanged(object sender, EventArgs e) {
            try {
                var message = (MailMessageContext)sender;
                ViewMessage(message);
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
    }
}