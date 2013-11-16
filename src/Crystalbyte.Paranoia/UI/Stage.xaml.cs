﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public class Stage : ItemsControl {
        public Stage() {
            DefaultStyleKey = typeof (Stage);
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is StageItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new StageItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
        }

        private void CenterZoomAroundMousePosition() {
            var p = Mouse.GetPosition(this);
            MouseX = p.X;
            MouseY = p.Y;
        }

        public double Zoom {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Zoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(Stage), new PropertyMetadata(1.0d));


        public double MouseX {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register("MouseX", typeof(double), typeof(Stage), new PropertyMetadata(0.0d));


        public double MouseY {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register("MouseY", typeof(double), typeof(Stage), new PropertyMetadata(0.0d));

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                CenterZoomAroundMousePosition();
                ApplyZoomFactor(e.Delta);    
            }
            base.OnMouseWheel(e);
        }

        private void ApplyZoomFactor(double delta) {
            Zoom += delta/1000;
            if (Zoom < 0.1d) {
                Zoom = 0.1d;
            }
        }
    }
}
