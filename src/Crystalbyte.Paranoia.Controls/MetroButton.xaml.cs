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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public sealed class MetroButton : Button {
        #region Construction

        static MetroButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MetroButton),
                new FrameworkPropertyMetadata(typeof (MetroButton)));
        }

        #endregion

        #region Dependency Properties

        public ImageSource ImageSource {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (MetroButton),
                new PropertyMetadata(null));

        public Brush AccentBrush {
            get { return (Brush) GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof (Brush), typeof (MetroButton), new PropertyMetadata(null));

        public Brush ImageBrush {
            get { return (Brush) GetValue(ImageBrushProperty); }
            set { SetValue(ImageBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageBrushProperty =
            DependencyProperty.Register("ImageBrush", typeof (Brush), typeof (MetroButton), new PropertyMetadata(null));

        #endregion
    }
}