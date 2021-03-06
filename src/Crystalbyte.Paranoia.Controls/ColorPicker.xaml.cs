﻿#region Copyright Notice & Copying Permission

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
using System.Windows.Controls.Primitives;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = ColorSourcePartName, Type = typeof (ListView))]
    [TemplatePart(Name = ButtonPartName, Type = typeof (ToggleButton))]
    public sealed class ColorPicker : ContentControl {
        #region Xaml Support

        public const string ColorSourcePartName = "PART_ColorSelector";
        public const string ButtonPartName = "PART_Button";

        #endregion

        #region Private Fields

        private ListView _colorSelector;
        private ToggleButton _button;

        #endregion

        #region Construction

        static ColorPicker() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ColorPicker),
                new FrameworkPropertyMetadata(typeof (ColorPicker)));
        }

        #endregion

        #region Public Events

        public event EventHandler SelectedColorChanged;

        private void OnSelectedColorChanged() {
            var handler = SelectedColorChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Class Override

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _button = (ToggleButton) Template.FindName(ButtonPartName, this);
            _colorSelector = (ListView) Template.FindName(ColorSourcePartName, this);
            _colorSelector.SelectionChanged += OnColorSourceSelectionChanged;
        }

        private void OnColorSourceSelectionChanged(object sender, SelectionChangedEventArgs e) {
            SelectedColor = (Color) _colorSelector.SelectedValue;
            OnSelectedColorChanged();
            _button.IsChecked = false;
        }

        #endregion

        #region Dependency Properties

        public object AvailableColors {
            get { return GetValue(AvailableColorsProperty); }
            set { SetValue(AvailableColorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AvailableColors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AvailableColorsProperty =
            DependencyProperty.Register("AvailableColors", typeof (object), typeof (ColorPicker),
                new PropertyMetadata(null));

        public Color SelectedColor {
            get { return (Color) GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof (Color), typeof (ColorPicker),
                new PropertyMetadata(Colors.Black));

        public Brush PopupBackground {
            get { return (Brush) GetValue(PopupBackgroundProperty); }
            set { SetValue(PopupBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupBackgroundProperty =
            DependencyProperty.Register("PopupBackground", typeof (Brush), typeof (ColorPicker),
                new PropertyMetadata(null));

        public Brush PopupBorderBrush {
            get { return (Brush) GetValue(PopupBorderBrushProperty); }
            set { SetValue(PopupBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupBorderBrushProperty =
            DependencyProperty.Register("PopupBorderBrush", typeof (Brush), typeof (ColorPicker),
                new PropertyMetadata(null));

        public DataTemplate ColorTemplate {
            get { return (DataTemplate) GetValue(ColorTemplateProperty); }
            set { SetValue(ColorTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorTemplateProperty =
            DependencyProperty.Register("ColorTemplate", typeof (DataTemplate), typeof (ColorPicker),
                new PropertyMetadata(null));

        #endregion
    }
}