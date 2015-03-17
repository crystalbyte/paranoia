#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Sandbox
// 
// Crystalbyte.Paranoia.Sandbox is free software: you can redistribute it and/or modify
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

using System.ComponentModel;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia.Sandbox {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            // Initialization must be performed here,
            // before creating a WebControl.
            //if (!WebCore.IsInitialized) {
            //    WebCore.Initialize(new WebConfig {
            //        HomeURL = "http://www.awesomium.com".ToUri(),
            //    });
            //}

            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }

            SuggestionBox.ItemsSourceRequested += OnItemsSourceRequested;
        }

        private void OnItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            SuggestionBox.ItemsSource = new[] {"Alexander Wieser", "Marvin Schluch", "Sebastian Thobe"};
        }
    }
}