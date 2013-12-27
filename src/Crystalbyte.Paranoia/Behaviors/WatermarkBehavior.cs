#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.Behaviors {
    /// <summary>
    ///   Class that provides the Watermark attached property
    /// </summary>
    internal sealed class WatermarkBehavior  {
        /// <summary>
        ///   Watermark Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark",
            typeof (object),
            typeof (WatermarkBehavior),
            new FrameworkPropertyMetadata(null, OnWatermarkChanged));

        #region Private Fields

        /// <summary>
        ///   Dictionary of ItemsControls
        /// </summary>
        private static readonly Dictionary<object, ItemsControl> ItemsControls = new Dictionary<object, ItemsControl>();

        #endregion

        /// <summary>
        ///   Gets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"> <see cref="DependencyObject" /> to get the property from </param>
        /// <returns> The value of the Watermark property </returns>
        public static object GetWatermark(DependencyObject d) {
            return d.GetValue(WatermarkProperty);
        }

        /// <summary>
        ///   Sets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"> <see cref="DependencyObject" /> to set the property on </param>
        /// <param name="value"> value of the property </param>
        public static void SetWatermark(DependencyObject d, object value) {
            d.SetValue(WatermarkProperty, value);
        }

        /// <summary>
        ///   Handles changes to the Watermark property.
        /// </summary>
        /// <param name="d"> <see cref="DependencyObject" /> that fired the event </param>
        /// <param name="e"> A <see cref="DependencyPropertyChangedEventArgs" /> that contains the event data. </param>
        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (Control) d;
            control.Loaded += OnControlLostKeyboardFocus;

            if (d is ComboBox || d is TextBox || d is PasswordBox) {
                control.GotKeyboardFocus += OnControlGotKeyboardFocus;
                control.LostKeyboardFocus += OnControlLostKeyboardFocus;
            }

            if (control is TextBox) {
                var textbox = control as TextBox;
                textbox.TextChanged += OnTextBoxTextChanged;
            }

            if (control is PasswordBox) {
                var textbox = control as PasswordBox;
                textbox.PasswordChanged += OnPasswordChanged;
            }

            if (!(d is ItemsControl) || d is ComboBox) 
                return;

            var i = (ItemsControl) d;

            // for Items property  
            i.ItemContainerGenerator.ItemsChanged += OnItemsChanged;
            ItemsControls.Add(i.ItemContainerGenerator, i);

            // for ItemsSource property  
            var prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
            prop.AddValueChanged(i, OnItemsSourceChanged);
        }

        #region Event Handlers

        private static void OnPasswordChanged(object sender, EventArgs e) {
            var c = (PasswordBox)sender;
            if (ShouldShowWatermark(c)) {
                ShowWatermark(c);
            } else {
                RemoveWatermark(c);
            }
        }

        private static void OnTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            var c = (TextBox)sender;
            if (ShouldShowWatermark(c)) {
                ShowWatermark(c);
            } else {
                RemoveWatermark(c);
            }
        }

        /// <summary>
        ///   Handle the GotFocus event on the control
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> A <see cref="RoutedEventArgs" /> that contains the event data. </param>
        private static void OnControlGotKeyboardFocus(object sender, RoutedEventArgs e) {
            var c = (Control) sender;
            RemoveWatermark(c);
        }

        /// <summary>
        ///   Handle the Loaded and LostFocus event on the control
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> A <see cref="RoutedEventArgs" /> that contains the event data. </param>
        private static void OnControlLostKeyboardFocus(object sender, RoutedEventArgs e) {
            var control = (Control) sender;
            if (!ShouldShowWatermark(control)) 
                return;

            ShowWatermark(control);
            Debug.WriteLine(string.Format("+ Watermark @ {0}. Focused = {1}.", control.Name, control.IsFocused));
        }

        /// <summary>
        ///   Event handler for the items source changed event
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> A <see cref="EventArgs" /> that contains the event data. </param>
        private static void OnItemsSourceChanged(object sender, EventArgs e) {
            var c = (ItemsControl) sender;
            if (c.ItemsSource != null) {
                if (ShouldShowWatermark(c)) {
                    ShowWatermark(c);
                }
                else {
                    RemoveWatermark(c);
                }
            }
            else {
                ShowWatermark(c);
            }
        }

        /// <summary>
        ///   Event handler for the items changed event
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> A <see cref="ItemsChangedEventArgs" /> that contains the event data. </param>
        private static void OnItemsChanged(object sender, ItemsChangedEventArgs e) {
            ItemsControl control;
            if (!ItemsControls.TryGetValue(sender, out control))
                return;

            if (ShouldShowWatermark(control)) {
                ShowWatermark(control);
            }
            else {
                RemoveWatermark(control);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        ///   Remove the watermark from the specified element
        /// </summary>
        /// <param name="control"> Element to remove the watermark from </param>
        private static void RemoveWatermark(UIElement control) {
            var layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer == null)
                return;

            var adorners = layer.GetAdorners(control);
            if (adorners == null) {
                return;
            }

            foreach (var adorner in adorners.OfType<WatermarkAdorner>()) {
                adorner.Visibility = Visibility.Hidden;
                layer.Remove(adorner);
            }
        }

        /// <summary>
        ///   Show the watermark on the specified control
        /// </summary>
        /// <param name="control"> Control to show the watermark on </param>
        private static void ShowWatermark(UIElement control) {
            var layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null) {
                layer.Add(new WatermarkAdorner(control, GetWatermark(control)));
            }
        }

        /// <summary>
        ///   Indicates whether or not the watermark should be shown on the specified control
        /// </summary>
        /// <param name="c"> <see cref="Control" /> to test </param>
        /// <returns> true if the watermark should be shown; false otherwise </returns>
        private static bool ShouldShowWatermark(IInputElement c) {
            if (c is ComboBox) {
                return (c as ComboBox).Text == string.Empty;
            }
            if (c is PasswordBox) {
                return (c as PasswordBox).Password == string.Empty && Keyboard.FocusedElement != c;
            }
            if (c is TextBox) {
                return (c as TextBox).Text == string.Empty && Keyboard.FocusedElement != c;
                
            }
            if (c is ItemsControl) {
                return (c as ItemsControl).Items.Count == 0;
            }

            return false;
        }

        #endregion
    }
}