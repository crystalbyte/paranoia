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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     A metrofied MetroProgressBar.
    ///     <see cref="MetroProgressBar" />
    /// </summary>
    public class MetroProgressBar : ProgressBar {
        public static readonly DependencyProperty EllipseDiameterProperty =
            DependencyProperty.Register("EllipseDiameter", typeof (double), typeof (MetroProgressBar),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty EllipseOffsetProperty =
            DependencyProperty.Register("EllipseOffset", typeof (double), typeof (MetroProgressBar),
                new PropertyMetadata(default(double)));

        static MetroProgressBar() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MetroProgressBar),
                new FrameworkPropertyMetadata(typeof (MetroProgressBar)));
            IsIndeterminateProperty.OverrideMetadata(typeof (MetroProgressBar),
                new FrameworkPropertyMetadata(OnIsIndeterminateChanged));
        }

        public MetroProgressBar() {
            SizeChanged += SizeChangedHandler;
            IsVisibleChanged += VisibleChangedHandler;
        }

        private void VisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e) {
            //reset Storyboard if Visibility is set to Visible #1300
            if (e.NewValue is bool && (bool) e.NewValue) {
                ResetStoryboard(ActualWidth);
            }
        }

        private static void OnIsIndeterminateChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e) {
            var bar = dependencyObject as MetroProgressBar;
            if (bar == null || e.NewValue == e.OldValue)
                return;

            var indeterminateState = bar.GetIndeterminate();
            var containingObject = bar.GetTemplateChild("ContainingGrid") as FrameworkElement;
            if (indeterminateState == null || containingObject == null)
                return;

            if ((bool) e.NewValue) {
                indeterminateState.Storyboard.Begin(containingObject, true);
            }
            else {
                indeterminateState.Storyboard.Stop(containingObject);
            }
        }

        /// <summary>
        ///     Gets/sets the diameter of the ellipses used in the indeterminate animation.
        /// </summary>
        public double EllipseDiameter {
            get { return (double) GetValue(EllipseDiameterProperty); }
            set { SetValue(EllipseDiameterProperty, value); }
        }

        /// <summary>
        ///     Gets/sets the offset of the ellipses used in the indeterminate animation.
        /// </summary>
        public double EllipseOffset {
            get { return (double) GetValue(EllipseOffsetProperty); }
            set { SetValue(EllipseOffsetProperty, value); }
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e) {
            var actualWidth = ActualWidth;
            var bar = this;
            if (Visibility == Visibility.Visible) {
                bar.ResetStoryboard(actualWidth);
            }
        }

        private void ResetStoryboard(double width) {
            //perform calculations
            var containerAnimStart = CalcContainerAnimStart(width);
            var containerAnimEnd = CalcContainerAnimEnd(width);
            var ellipseAnimWell = CalcEllipseAnimWell(width);
            var ellipseAnimEnd = CalcEllipseAnimEnd(width);
            //reset the main double animation
            var indeterminate = GetIndeterminate();

            if (indeterminate == null)
                return;

            var newStoryboard = indeterminate.Storyboard.Clone();
            var doubleAnim = newStoryboard.Children.First(t => t.Name == "MainDoubleAnim");
            doubleAnim.SetValue(DoubleAnimation.FromProperty, containerAnimStart);
            doubleAnim.SetValue(DoubleAnimation.ToProperty, containerAnimEnd);

            var namesOfElements = new[] {"E1", "E2", "E3", "E4", "E5"};
            foreach (var elemName in namesOfElements) {
                var doubleAnimParent =
                    (DoubleAnimationUsingKeyFrames) newStoryboard.Children.First(t => t.Name == elemName + "Anim");
                DoubleKeyFrame first, second, third;
                if (elemName == "E1") {
                    first = doubleAnimParent.KeyFrames[1];
                    second = doubleAnimParent.KeyFrames[2];
                    third = doubleAnimParent.KeyFrames[3];
                }
                else {
                    first = doubleAnimParent.KeyFrames[2];
                    second = doubleAnimParent.KeyFrames[3];
                    third = doubleAnimParent.KeyFrames[4];
                }

                first.Value = ellipseAnimWell;
                second.Value = ellipseAnimWell;
                third.Value = ellipseAnimEnd;
                first.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                second.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                third.InvalidateProperty(DoubleKeyFrame.ValueProperty);

                doubleAnimParent.InvalidateProperty(Storyboard.TargetPropertyProperty);
                doubleAnimParent.InvalidateProperty(Storyboard.TargetNameProperty);
            }

            indeterminate.Storyboard.Remove();
            indeterminate.Storyboard = newStoryboard;

            if (!IsIndeterminate) {
                return;
            }

            var grid = (FrameworkElement) GetTemplateChild("ContainingGrid");
            if (grid == null) {
                return;
            }

            indeterminate.Storyboard.Begin(grid, true);
        }

        private VisualState GetIndeterminate() {
            var templateGrid = GetTemplateChild("ContainingGrid") as FrameworkElement;
            if (templateGrid == null) {
                return null;
            }
            var groups = VisualStateManager.GetVisualStateGroups(templateGrid);
            return groups != null
                ? groups.Cast<VisualStateGroup>()
                    .SelectMany(@group => @group.States.Cast<VisualState>())
                    .FirstOrDefault(state => state.Name == "Indeterminate")
                : null;
        }


        private void SetEllipseDiameter(double width) {
            if (width <= 180) {
                EllipseDiameter = 4;
                return;
            }
            if (width <= 280) {
                EllipseDiameter = 5;
                return;
            }

            EllipseDiameter = 6;
        }

        private void SetEllipseOffset(double width) {
            if (width <= 180) {
                EllipseOffset = 4;
                return;
            }
            if (width <= 280) {
                EllipseOffset = 7;
                return;
            }

            EllipseOffset = 9;
        }

        private static double CalcContainerAnimStart(double width) {
            if (width <= 180)
                return -34;
            if (width <= 280)
                return -50.5;

            return -63;
        }

        private static double CalcContainerAnimEnd(double width) {
            var firstPart = 0.4352*width;
            if (width <= 180)
                return firstPart - 25.731;
            if (width <= 280)
                return firstPart + 27.84;

            return firstPart + 58.862;
        }

        private static double CalcEllipseAnimWell(double width) {
            return width*1.0/3.0;
        }

        private static double CalcEllipseAnimEnd(double width) {
            return width*2.0/3.0;
        }


        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            SizeChangedHandler(null, null);
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            // Update the Ellipse properties to their default values
            // only if they haven't been user-set.
            if (EllipseDiameter.Equals(0))
                SetEllipseDiameter(ActualWidth);
            if (EllipseOffset.Equals(0))
                SetEllipseOffset(ActualWidth);
        }
    }
}