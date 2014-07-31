﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for WriteMessagePage.xaml
    /// </summary>
    public partial class ComposeMessagePage : INavigationAware {

        public ComposeMessagePage() {
            DataContext = new MailCompositionContext();
            InitializeComponent();

            var window = (MainWindow)Application.Current.MainWindow;
            window.OverlayChanged += OnOverlayChanged;
        }

        private async void Reset() {
            var composition = (MailCompositionContext) DataContext;
            await composition.ResetAsync();

            SuggestionBox.Focus();
        }

        private void OnOverlayChanged(object sender, EventArgs e) {
            var window = (MainWindow)Application.Current.MainWindow;
            if (!window.IsOverlayVisible) {
                SuggestionBox.Close();
            }
        }

        public MailCompositionContext Composition {
            get { return (MailCompositionContext) DataContext; }
        }

        private async void OnAutoCompleteBoxOnItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            await Composition.QueryRecipientsAsync(e.Text);
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            Reset();
        }

        #endregion
    }
}
