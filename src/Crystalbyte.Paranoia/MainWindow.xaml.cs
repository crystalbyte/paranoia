﻿#region Using directives

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Pages;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        #region Private Fields

        private Storyboard _slideInOverlayStoryboard;
        private Storyboard _slideOutOverlayStoryboard;

        #endregion

        #region Construction

        public MainWindow() {
            InitializeComponent();
            DataContext = App.Context;

            CommandBindings.Add(new CommandBinding(WindowCommands.CloseFlyOut, OnCloseFlyOut));
            CommandBindings.Add(new CommandBinding(WindowCommands.OpenAccountMenu, OnOpenAccountMenu));

            if (DesignerProperties.GetIsInDesignMode(this)) {
                HtmlControl.Visibility = Visibility.Collapsed;
            }

            Loaded += OnLoaded;
        }

        private void OnOpenAccountMenu(object sender, ExecutedRoutedEventArgs e) {

        }

        private static async void OnLoaded(object sender, RoutedEventArgs e) {
            await EnsureKeyExistenceAsync();
        }

        private static async Task EnsureKeyExistenceAsync() {
            var keyDir = AppContext.GetKeyDirectory();
            if (!keyDir.Exists) {
                await CreateKeyDirectoryAsync(keyDir);
            }

            var publicKey = keyDir.GetFiles(Settings.Default.PublicKeyFile).FirstOrDefault();
            var privateKey = keyDir.GetFiles(Settings.Default.PrivateKeyFile).FirstOrDefault();

            if (publicKey == null || privateKey == null) {
                App.Context.OnCreateKeyPair();
            } else {
                await App.Context.InitKeysAsync();
                await App.Context.RunAsync();
            }
        }

        private static Task CreateKeyDirectoryAsync(DirectoryInfo keyDir) {
            return Task.Factory.StartNew(() => {
                var identity = new NTAccount(Environment.UserDomainName,
                    Environment.UserName);
                var security = new DirectorySecurity();
                security.PurgeAccessRules(identity);

                security.AddAccessRule(new FileSystemAccessRule(identity,
                    FileSystemRights.Read, AccessControlType.Allow));
                security.AddAccessRule(new FileSystemAccessRule(identity,
                    FileSystemRights.Write, AccessControlType.Allow));
                security.AddAccessRule(new FileSystemAccessRule(identity,
                    FileSystemRights.Modify, AccessControlType.Allow));

                keyDir.Create(security);
            });
        }

        private static void OnCloseFlyOut(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyOut();
        }

        #endregion

        #region Public Events

        public event EventHandler FlyOutVisibilityChanged;

        private void OnFlyOutVisibilityChanged() {
            var handler = FlyOutVisibilityChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Class Overrides

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            LoadResources();
            HookUpEvents();
        }

        private void LoadResources() {
            _slideInOverlayStoryboard = (Storyboard)Resources["OverlaySlideInStoryboard"];
            _slideInOverlayStoryboard.Completed += OnSlideInOverlayCompleted;
            _slideOutOverlayStoryboard = (Storyboard)Resources["OverlaySlideOutStoryboard"];
            _slideOutOverlayStoryboard.Completed += OnSlideOutOverlayCompleted;
        }

        private void OnSlideInOverlayCompleted(object sender, EventArgs e) {
            var page = FlyOutFrame.Content as IAnimationAware;
            if (page != null) {
                page.OnAnimationFinished();
            }
        }

        private void OnSlideOutOverlayCompleted(object sender, EventArgs e) {
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void HookUpEvents() {
            App.Context.PopupNavigationRequested += OnPopupNavigationRequested;
            App.Context.FlyOutNavigationRequested += OnFlyOutNavigationRequested;
            App.Context.SortOrderChanged += OnSortOrderChanged;
        }

        private void OnSortOrderChanged(object sender, EventArgs e) {
            var source = Resources["MessagesSource"] as CollectionViewSource;
            if (source == null) 
                return;

            source.SortDescriptions.Clear();
            source.SortDescriptions.Add(new SortDescription("EntryDate", App.Context.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
        }

        private void OnPopupNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            PopupFrame.Navigate(e.Target);
        }

        private void OnFlyOutNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            FlyOutFrame.Navigate(e.Target);
            if (e.Target == typeof(BlankPage).ToPageUri()) {
                HideOverlay();
            } else {
                ShowOverlay();
            }
        }

        private void ShowOverlay() {
            IsFlyOutVisible = true;
            Overlay.Visibility = Visibility.Visible;
            _slideInOverlayStoryboard.Begin();
        }

        private void HideOverlay() {
            IsFlyOutVisible = false;
            while (FlyOutFrame.CanGoBack) {
                FlyOutFrame.NavigationService.RemoveBackEntry();
            }

            _slideOutOverlayStoryboard.Begin();
        }

        #endregion

        #region Dependency Properties

        public bool IsFlyOutVisible {
            get { return (bool)GetValue(IsFlyOutVisibleProperty); }
            set { SetValue(IsFlyOutVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOverlayVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlyOutVisibleProperty =
            DependencyProperty.Register("IsFlyOutVisible", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false, OnIsOverlayChanged));

        #endregion

        private static void OnIsOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (MainWindow)d;
            window.OnFlyOutVisibilityChanged();
        }

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var app = App.Context;
            app.OnMessageSelectionChanged();

            var message = app.SelectedMessage;
            if (message == null)
                return;

            var container = (Control)MessagesListView.ItemContainerGenerator.ContainerFromItem(message);
            if (container != null) {
                container.Focus();
            }
        }

        private void OnFlyOutFrameNavigated(object sender, NavigationEventArgs e) {
            var page = FlyOutFrame.Content as INavigationAware;
            if (page != null) {
                page.OnNavigated(e);
            }
        }
        
        private async void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var tree = (TreeView) sender;
            var value = tree.SelectedValue;

            var mailbox = value as MailboxContext;
            if (mailbox != null) {
                App.Context.SelectedMailbox = mailbox;
            }

            var account = value as MailAccountContext;
            if (account == null) 
                return;

            if (!account.IsOnline) {
                await account.TakeOnlineAsync();
            }
        }

        private void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            var view = (ListView) sender;
            var attachment = (AttachmentContext)view.SelectedValue;
            if (attachment == null) {
                return;
            }
            attachment.Open();
        }
    }
}