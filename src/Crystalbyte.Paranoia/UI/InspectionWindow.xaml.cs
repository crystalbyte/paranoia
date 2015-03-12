using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Crystalbyte.Paranoia.Themes;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for InspectionWindow.xaml
    /// </summary>
    public partial class InspectionWindow : IAccentAware {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public InspectionWindow() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoreDown));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));

            CommandBindings.Add(new CommandBinding(MessageCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessageCommands.ReplyAll, OnReplyAll));
            CommandBindings.Add(new CommandBinding(MessageCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint));
        }

        #endregion

        #region Class Overrides

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            var message = DataContext as MailMessageContext;
            if (message == null)
                return;

            // Remove handler to cut the reference from the message to this window.
            message.AllowExternalContentChanged -= OnMessageTrustChanged;
        }

        #endregion

        #region Methods

        private void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            if (!IsLoaded) {
                return;
            }
            var view = (ListView)sender;
            var attachment = (AttachmentContext)view.SelectedValue;
            if (attachment == null) {
                return;
            }
            attachment.Open();
        }

        private async void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            try {
                await HtmlViewer.PrintAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
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
                Logger.Error(ex);
            }
        }

        public void InitWithMessage(MailMessageContext message) {
            try {
                DataContext = message;
                if (message.IsInitialized) {
                    message.AllowExternalContentChanged += OnMessageTrustChanged;
                    ViewMessage(message);
                    return;
                }

                message.Initialized += OnMessageInitialized;

            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnMessageTrustChanged(object sender, EventArgs e) {
            var message = (MailMessageContext)sender;
            ViewMessage(message);
        }

        private void OnMessageInitialized(object sender, EventArgs e) {
            var message = (MailMessageContext)sender;
            message.Initialized -= OnMessageInitialized;

            message.AllowExternalContentChanged += OnMessageTrustChanged;
            ViewMessage(message);
        }

        private void ViewMessage(MailMessageContext message) {
            HtmlViewer.Source = string.Format(message.IsExternalContentAllowed
                    ? "message:///{0}?blockExternals=false"
                    : "message:///{0}", message.Id);
        }

        public void InitWithFile(FileMessageContext file) {
            try {
                DataContext = file;
                HtmlViewer.Source = string.Format("file:///local?path={0}",
                    Uri.EscapeDataString(file.FullName));
            } catch (Exception ex) {
                Logger.Error(ex);
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
