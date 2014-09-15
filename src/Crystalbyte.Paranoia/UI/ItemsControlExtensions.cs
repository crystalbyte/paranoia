﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Crystalbyte.Paranoia.UI {
    public static class ItemsControlExtensions {
        public static void ScrollToCenterOfView(this ItemsControl itemsControl, object item) {
            // Scroll immediately if possible
            if (itemsControl.TryScrollToCenterOfView(item))
                return;

            // Otherwise wait until everything is loaded, then scroll
            var box = itemsControl as ListBox;
            if (box != null) box.ScrollIntoView(item);
            itemsControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => itemsControl.TryScrollToCenterOfView(item)));
        }

        private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, object item) {
            // Find the container
            var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
            if (container == null) return false;

            // Find the ScrollContentPresenter
            ScrollContentPresenter presenter = null;
            for (Visual vis = container; vis != null && !Equals(vis, itemsControl); vis = VisualTreeHelper.GetParent(vis) as Visual)
                if ((presenter = vis as ScrollContentPresenter) != null)
                    break;
            if (presenter == null) return false;

            // Find the IScrollInfo
            var scrollInfo =
                !presenter.CanContentScroll ? presenter :
                    presenter.Content as IScrollInfo ??
                    FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo ??
                    presenter;

            // Compute the center point of the container relative to the scrollInfo
            var size = container.RenderSize;
            var center = container.TransformToAncestor((Visual)scrollInfo).Transform(new Point(size.Width / 2, size.Height / 2));
            center.Y += scrollInfo.VerticalOffset;
            center.X += scrollInfo.HorizontalOffset;

            // Adjust for logical scrolling
            if (scrollInfo is StackPanel || scrollInfo is VirtualizingStackPanel) {
                var logicalCenter = itemsControl.ItemContainerGenerator.IndexFromContainer(container) + 0.5;
                var orientation = scrollInfo is StackPanel ? ((StackPanel)scrollInfo).Orientation : ((VirtualizingStackPanel)scrollInfo).Orientation;
                if (orientation == Orientation.Horizontal)
                    center.X = logicalCenter;
                else
                    center.Y = logicalCenter;
            }

            // Scroll the center of the container to the center of the viewport
            if (scrollInfo.CanVerticallyScroll) scrollInfo.SetVerticalOffset(CenteringOffset(center.Y, scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));
            if (scrollInfo.CanHorizontallyScroll) scrollInfo.SetHorizontalOffset(CenteringOffset(center.X, scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));
            return true;
        }

        private static double CenteringOffset(double center, double viewport, double extent) {
            return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
        }
        private static DependencyObject FirstVisualChild(DependencyObject visual) {
            if (visual == null) return null;
            return VisualTreeHelper.GetChildrenCount(visual) == 0 ? null : VisualTreeHelper.GetChild(visual, 0);
        }
    }
}