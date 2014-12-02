using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Crystalbyte.Paranoia.UI {
    public static class WindowExtensions {
        public static void MimicOwnership(this Window window, Window owner) {
            window.Owner = owner;
            window.Height = owner.Height > 500 ? owner.Height * 0.9 : 500;
            window.Width = owner.Width > 800 ? owner.Width * 0.9 : 800;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Loaded += OnWindowLoaded;
            owner.Closed += (sender, e) => window.Close();
        }

        private static void OnWindowLoaded(object sender, RoutedEventArgs e) {
            var window = (Window)sender;
            window.Loaded -= OnWindowLoaded;

            var timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(400)
            };

            timer.Tick += (x, y) => {
                timer.Stop();
                window.Owner = null;
            };

            timer.Start();
        }
    }
}
