#region Using directives

using System;
using System.Windows;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia {
    internal static class ApplicationExtensions {
        public static void AssertUIThread(this Application app) {
            if (!app.Dispatcher.CheckAccess()) {
                throw new InvalidOperationException(Resources.CrossThreadException);
            }
        }

        public static void AssertBackgroundThread(this Application app) {
            if (app.Dispatcher.CheckAccess()) {
                throw new InvalidOperationException(Resources.CrossThreadException);
            }
        }
    }
}