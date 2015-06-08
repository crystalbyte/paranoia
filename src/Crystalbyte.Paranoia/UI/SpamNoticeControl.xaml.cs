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

using System;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for SpamNoticeControl.xaml
    /// </summary>
    public partial class SpamNoticeControl {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public SpamNoticeControl() {
            InitializeComponent();
        }

        #endregion

        #region Dependency Properties

        public IAuthenticatable Authenticatable {
            get { return (IAuthenticatable)GetValue(AuthenticatableProperty); }
            set { SetValue(AuthenticatableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Authenticatable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AuthenticatableProperty =
            DependencyProperty.Register("Authenticatable", typeof(IAuthenticatable), typeof(SpamNoticeControl), new PropertyMetadata(OnAuthenticatableChanged));

        private static void OnAuthenticatableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            try {
                var control = (SpamNoticeControl)d;

                var oldAuth = e.OldValue as IAuthenticatable;
                if (oldAuth != null) {
                    control.Detach(oldAuth);
                }

                var newAuth = e.NewValue as IAuthenticatable;
                if (newAuth == null) 
                    return;

                control.Attach(newAuth);
                control.Refresh(newAuth);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void Detach(IAuthenticatable auth) {
            auth.AuthenticityChanged -= OnAuthenticityChanged;
        }

        private void Attach(IAuthenticatable auth) {
            auth.AuthenticityChanged += OnAuthenticityChanged;
        }

        private void Refresh(IAuthenticatable auth) {
            Visibility = auth.Authenticity == Authenticity.Unspecified
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void OnAuthenticityChanged(object sender, EventArgs e) {
            try {
                if (Authenticatable != null) {
                    Refresh(Authenticatable);
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Methods

        private async void OnChangeAuthenticity(object sender, ExecutedRoutedEventArgs e) {
            try {
                if (e.Parameter == (object)Authenticity.Confirmed) {
                    await Authenticatable.ConfirmAsync();
                } else {
                    await Authenticatable.RejectAsync();
                }

                Refresh(Authenticatable);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion
    }
}