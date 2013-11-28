using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Behaviors {
    internal sealed class InitialFocusBehavior : DependencyObject {

        /// <summary>
        ///   IsEnabled Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(InitialFocusBehavior),
            new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        ///   Gets the IeEnabled property.  This dependency property indicates the state for the control.
        /// </summary>
        /// <param name="d"> <see cref="DependencyObject" /> to get the property from </param>
        /// <returns> The value of the Watermark property </returns>
        public static bool GetIsEnabled(DependencyObject d) {
            return (bool) d.GetValue(IsEnabledProperty);
        }

        /// <summary>
        ///   Sets the IsEnabled property.  This dependency property indicates the state for the control.
        /// </summary>
        /// <param name="d"> <see cref="DependencyObject" /> to set the property on </param>
        /// <param name="value"> value of the property </param>
        public static void SetIsEnabled(DependencyObject d, bool value) {
            d.SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        ///   Handles changes to the IsEnabled property.
        /// </summary>
        /// <param name="d"> <see cref="DependencyObject" /> that fired the event </param>
        /// <param name="e"> A <see cref="DependencyPropertyChangedEventArgs" /> that contains the event data. </param>
        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var element = d as FrameworkElement;
            if (element == null) {
                Debug.WriteLine("Can't attach to non visual.");
                return;
            }

            var enabled = (bool) e.NewValue;
            if (enabled) {
                element.IsVisibleChanged += OnFrameworkElementIsVisibleChanged;
            } else {
                element.IsVisibleChanged -= OnFrameworkElementIsVisibleChanged;
            }
        }

        private static void OnFrameworkElementIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var element = (FrameworkElement)sender;
            if (element.IsVisible) {
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));    
            }
        }
    }
}
