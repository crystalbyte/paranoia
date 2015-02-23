﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

            Deactivated += (sender, e) => Debug.WriteLine("deactivated");
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

        private void OnForward(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (InspectionContext)DataContext;
                context.Forward();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnReply(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (InspectionContext)DataContext;
                context.ReplyAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (InspectionContext)DataContext;
                context.ReplyAll();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task InitWithMessageAsync(MailMessageContext message) {
            var context = new MessageInspectionContext(message);
            try {
                DataContext = context;
                HtmlViewer.Source = string.Format(message.IsSourceTrusted
                    ? "message:///{0}?blockExternals=false"
                    : "message:///{0}", message.Id);

                await context.InitAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task InitWithFileAsync(FileSystemInfo file) {
            var context = new FileInspectionContext(file);
            try {
                DataContext = context;
                HtmlViewer.Source = string.Format("file:///local?path={0}", Uri.EscapeDataString(file.FullName));
                await context.InitAsync();
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
