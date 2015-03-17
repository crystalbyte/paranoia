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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateMailboxModalPage.xaml
    /// </summary>
    public partial class InsertLinkModalPage {
        public InsertLinkModalPage() {
            InitializeComponent();
            var parent = (HtmlEditor) NavigationArguments.Pop();
            DataContext = new InsertLinkContext(parent);

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Accept, OnAccept, OnCanAccept));
            NameTextBox.TextChanged += OnNameTextBoxTextChanged;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            NameTextBox.Focus();
        }

        private static void OnNameTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnCanAccept(object sender, CanExecuteRoutedEventArgs e) {
            var context = (InsertLinkContext) DataContext;
            e.CanExecute = context.IsValid;
        }

        private void Cancel() {
            var window = Window.GetWindow(this) as CompositionWindow;
            if (window != null) {
                window.CloseOverlay();
            }
        }

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            Cancel();
        }

        private async void OnAccept(object sender, ExecutedRoutedEventArgs e) {
            Cancel();

            var context = (InsertLinkContext) DataContext;
            await context.CommitAsync();
        }
    }
}