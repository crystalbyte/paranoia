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

using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;
using Crystalbyte.Paranoia.UI;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia {
    
    [Export, Shared]
    [Export(typeof(IViewManager))]
    public sealed class AppContext : NotificationObject, IViewManager {

        #region Private Fields
        
        private bool _isAnimating;
        private bool _isPopupVisible;
        private readonly ObservableCollection<View> _views;

        /// <summary>
        /// Creates the default class logger.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public AppContext() {
            _views = new ObservableCollection<View>();
        }

        #endregion

        #region Events

        internal event EventHandler Initialized;

        private void OnInitialized() {
            var handler = Initialized;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler FlyoutClosing;

        private void OnFlyoutClosing() {
            var handler = FlyoutClosing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler FlyoutClosed;

        private void OnFlyoutClosed() {
            var handler = FlyoutClosed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler FlyoutCloseRequested;

        private void OnFlyoutCloseRequested() {
            var handler = FlyoutCloseRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler<NavigationRequestedEventArgs> ModalNavigationRequested;

        private void OnModalNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = ModalNavigationRequested;
            if (handler != null)
                handler(this, e);
        }

        internal event EventHandler<NavigationRequestedEventArgs> FlyoutNavigationRequested;

        private void OnFlyoutNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = FlyoutNavigationRequested;
            if (handler != null) {
                handler(this, e);
            }
        }

        #endregion

        #region Properties

        public IEnumerable<View> Views {
            get { return _views; }
        }

        [ImportMany]
        public IEnumerable<Module> Modules { get; set; }

        [ImportMany]
        public IEnumerable<Theme> Themes { get; set; }

        internal void ConfigureAccount(MailAccountContext account) {
            if (account == null) {
                throw new ArgumentNullException("account");
            }

            NavigationArguments.Push(account);
            var uri = typeof(AccountPropertyFlyoutPage).ToPageUri();
            OnFlyoutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        public bool IsPopupVisible {
            get { return _isPopupVisible; }
            set {
                if (_isPopupVisible == value) {
                    return;
                }
                _isPopupVisible = value;
                RaisePropertyChanged(() => IsPopupVisible);
            }
        }

        public bool IsAnimating {
            get { return _isAnimating; }
            set {
                if (_isAnimating == value) {
                    return;
                }
                _isAnimating = value;
                RaisePropertyChanged(() => IsAnimating);
            }
        }

        public bool IsDebugBuild { get; set; }

        #endregion

        #region Methods

        internal void NavigateModalToPage(Uri uri) {
            OnModalNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = true;
        }

        internal void NavigateFlyoutToPage(Uri uri) {
            OnFlyoutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal T GetModule<T>() where T : Module {
            return (T)Modules.FirstOrDefault(x => x is T);
        }

        internal void ApplySettings() {
            var context = App.Context.GetModule<MailModule>();
            context.ZoomLevel = Settings.Default.ZoomLevel;
        }

        internal void ClosePopup() {
            OnModalNavigationRequested(new NavigationRequestedEventArgs(null));
            IsPopupVisible = false;
        }

        private static Task<bool> CheckKeyPairAsync() {
            return Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.KeyPairs.Any();
                }
            });
        }

        private static Task GenerateKeyPairAsync() {
            return Task.Run(() => {
                var crypto = new PublicKeyCrypto();
                var pair = new KeyPair {
                    PublicKey = crypto.PublicKey,
                    PrivateKey = crypto.PrivateKey,
                    Device = Environment.MachineName,
                    Date = DateTime.Now
                };

                using (var context = new DatabaseContext()) {
                    context.KeyPairs.Add(pair);
                    context.SaveChanges();
                }
            });
        }

        public async Task InitializeAsync() {
            Application.Current.AssertUIThread();

            try {
                if (!await CheckKeyPairAsync()) {
                    await GenerateKeyPairAsync();
                }

                var initializers = Modules.Select(x => x.InitializeAsync());
                await Task.WhenAll(initializers);

                OnInitialized();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal void CloseFlyout() {
            OnFlyoutClosing();
            OnFlyoutCloseRequested();
            OnFlyoutClosed();
        }

        #endregion

        #region Implementation of IViewManager

        public void RegisterView(View view) {
            Application.Current.AssertUIThread();
            _views.Add(view);
        }

        #endregion
    }
}