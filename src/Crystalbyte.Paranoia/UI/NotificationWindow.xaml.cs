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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow {
        private readonly IEnumerable<MailMessageContext> _messages;
        private Storyboard _entryStoryboard;
        private Storyboard _exitStoryboard;

        public NotificationWindow(ICollection<MailMessageContext> messages) {
            _messages = messages;

            InitializeComponent();

            throw new NotImplementedException();
            //DataContext = new NotificationWindowContext(messages);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            _entryStoryboard.Begin();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Left = SystemParameters.WorkArea.Width - Width - 20;
            Top = SystemParameters.WorkArea.Top + 20;

            _entryStoryboard = (Storyboard) Resources["EntryAnimation"];
            _entryStoryboard.Completed += OnEntryStoryboardCompleted;

            _exitStoryboard = (Storyboard) Resources["ExitAnimation"];
            _exitStoryboard.Completed += OnExitAnimationCompleted;
        }

        private void SlideOut() {
            _exitStoryboard.Begin();
        }

        private void OnEntryStoryboardCompleted(object sender, EventArgs e) {
            SlideOut();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            SlideOut();
        }

        private void OnWindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_messages.Any()) {
                return;
            }

            var module = App.Context.GetModule<MailModule>();
            module.DisplayMessage(_messages.First());
            SlideOut();
        }

        private void OnExitAnimationCompleted(object sender, EventArgs e) {
            Close();
        }
    }
}