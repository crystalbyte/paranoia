using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public sealed class DynamicContainerStyle {
        public static Style GetBaseStyle(DependencyObject obj) {
            return (Style)obj.GetValue(BaseStyleProperty);
        }

        public static void SetBaseStyle(DependencyObject obj, Style value) {
            obj.SetValue(BaseStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for BaseStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseStyleProperty =
            DependencyProperty.RegisterAttached("BaseStyle", typeof(Style), typeof(DynamicContainerStyle), new UIPropertyMetadata(StylesChanged));

        public static Style GetDerivedStyle(DependencyObject obj) {
            return (Style)obj.GetValue(DerivedStyleProperty);
        }

        public static void SetDerivedStyle(DependencyObject obj, Style value) {
            obj.SetValue(DerivedStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for DerivedStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DerivedStyleProperty =
            DependencyProperty.RegisterAttached("DerivedStyle", typeof(Style), typeof(DynamicContainerStyle), new UIPropertyMetadata(StylesChanged));

        private static void StylesChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            if (!(target is ItemsControl))
                throw new InvalidCastException("Target must be ItemsControl");

            var element = (ItemsControl)target;
            var styles = new List<Style>();
            var baseStyle = GetBaseStyle(target);
            if (baseStyle != null)
                styles.Add(baseStyle);

            var derivedStyle = GetDerivedStyle(target);
            if (derivedStyle != null)
                styles.Add(derivedStyle);

            element.ItemContainerStyle = MergeStyles(styles);
        }

        private static Style MergeStyles(IEnumerable<Style> styles) {
            var newStyle = new Style();

            foreach (var style in styles) {
                foreach (var setter in style.Setters)
                    newStyle.Setters.Add(setter);

                foreach (var trigger in style.Triggers)
                    newStyle.Triggers.Add(trigger);
            }

            return newStyle;
        }
    }
}