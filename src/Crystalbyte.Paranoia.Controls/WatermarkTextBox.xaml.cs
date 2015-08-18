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
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for WatermarkTextBox.xaml
    /// </summary>
    public class WatermarkTextBox : TextBox {
        #region Construction

        static WatermarkTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (WatermarkTextBox),
                new FrameworkPropertyMetadata(typeof (WatermarkTextBox)));
        }

        public WatermarkTextBox() {
            TextChanged += OnTextChanged;
        }

        #endregion

        #region Class Overrides

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnLostKeyboardFocus(e);
            InvalidateWatermark();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnGotKeyboardFocus(e);
            InvalidateWatermark();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            InvalidateWatermark();
        }

        #endregion

        #region Methods

        private void OnTextChanged(object sender, TextChangedEventArgs e) {
            InvalidateWatermark();
        }

        private void InvalidateWatermark() {
            IsWatermarkVisible = string.IsNullOrEmpty(Text) && !IsKeyboardFocusWithin;
        }

        #endregion

        #region Dependency Properties

        public object Watermark {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof (object), typeof (WatermarkTextBox),
                new PropertyMetadata(null));


        public DataTemplate WatermarkTemplate {
            get { return (DataTemplate) GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WatermarkTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkTemplateProperty =
            DependencyProperty.Register("WatermarkTemplate", typeof (DataTemplate), typeof (WatermarkTextBox),
                new PropertyMetadata(null));

        public Brush AccentBrush {
            get { return (Brush) GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof (Brush), typeof (WatermarkTextBox),
                new PropertyMetadata(null));

        public bool IsWatermarkVisible {
            get { return (bool) GetValue(IsWatermarkVisibleProperty); }
            set { SetValue(IsWatermarkVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsWatermarkVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWatermarkVisibleProperty =
            DependencyProperty.Register("IsWatermarkVisible", typeof (bool), typeof (WatermarkTextBox),
                new PropertyMetadata(false));

        #endregion
    }
}