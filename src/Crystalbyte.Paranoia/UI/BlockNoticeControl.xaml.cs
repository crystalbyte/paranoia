﻿#region Copyright Notice & Copying Permission

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
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for BlockNoticeControl.xaml
    /// </summary>
    public partial class BlockNoticeControl {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public BlockNoticeControl() {
            InitializeComponent();
        }

        #endregion

        #region Dependency Properties

        public IBlockable Blockable {
            get { return (IBlockable)GetValue(BlockableProperty); }
            set { SetValue(BlockableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Blockable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockableProperty =
            DependencyProperty.Register("Blockable", typeof(IBlockable), typeof(BlockNoticeControl), new PropertyMetadata(OnBlockableChanged));

        private static void OnBlockableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            try {
                var control = (BlockNoticeControl)d;

                var oldAuth = e.OldValue as IBlockable;
                if (oldAuth != null) {
                    control.Detach(oldAuth);
                }

                var newAuth = e.NewValue as IBlockable;
                if (newAuth == null) 
                    return;

                control.Attach(newAuth);
                control.Refresh(newAuth);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Methods

        private void Attach(IBlockable blockable) {
            blockable.IsExternalContentAllowedChanged += OnIsExternalContentAllowedChanged;
        }

        private void Detach(IBlockable blockable) {
            blockable.IsExternalContentAllowedChanged -= OnIsExternalContentAllowedChanged;
        }

        private void OnIsExternalContentAllowedChanged(object sender, EventArgs e) {
            try {
                if (Blockable != null) {
                    Refresh(Blockable);    
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void Refresh(IBlockable blockable) {
            Visibility = blockable.IsExternalContentAllowed
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }

        private async void OnUnblockExternalContent(object sender, ExecutedRoutedEventArgs e) {
            try {
                if (Blockable == null) 
                    return;

                await Blockable.UnblockAsync();
                var module = App.Context.GetModule<MailModule>();
                if (module.SelectedMessage == Blockable) {
                    await module.RefreshMessageViewAsync();
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion
    }
}