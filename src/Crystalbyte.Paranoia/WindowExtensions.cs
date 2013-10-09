#region Using directives

using System.Windows;

#endregion

namespace Crystalbyte.Paranoia {
    internal static class WindowExtensions {
        public static void CenterScreen(this Window window) {
            var centerX = SystemParameters.WorkArea.Width/2;
            var centerY = SystemParameters.WorkArea.Height/2;

            window.Left = centerX - window.Width/2;
            window.Top = centerY - window.Height/2;
        }

        public static void MoveTopLeft(this Window window) {
            window.Left = -window.BorderThickness.Left;
            window.Top = -window.BorderThickness.Top;
        }
    }
}