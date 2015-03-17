#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Controls
// 
// Crystalbyte.Paranoia.Controls is free software: you can redistribute it and/or modify
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
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public class MetroScrollViewer : ScrollViewer {
        #region Construction

        static MetroScrollViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MetroScrollViewer),
                new FrameworkPropertyMetadata(typeof (MetroScrollViewer)));
        }

        #endregion

        #region Dependency Properties

        public bool IsTop {
            get { return (bool) GetValue(IsTopProperty); }
            set { SetValue(IsTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTopProperty =
            DependencyProperty.Register("IsTop", typeof (bool), typeof (MetroScrollViewer), new PropertyMetadata(false));


        public bool IsBottom {
            get { return (bool) GetValue(IsBottomProperty); }
            set { SetValue(IsBottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsBottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBottomProperty =
            DependencyProperty.Register("IsBottom", typeof (bool), typeof (MetroScrollViewer),
                new PropertyMetadata(false));

        #endregion

        #region Class Overrides

        protected override void OnScrollChanged(ScrollChangedEventArgs e) {
            base.OnScrollChanged(e);

            var scrollViewer = (ScrollViewer) e.OriginalSource;
            IsTop = scrollViewer.ScrollableHeight > 0 && Math.Abs(scrollViewer.VerticalOffset) < double.Epsilon;
            IsBottom = scrollViewer.ScrollableHeight > 0 &&
                       Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ViewportHeight - scrollViewer.ExtentHeight) <
                       double.Epsilon;
        }

        #endregion
    }
}