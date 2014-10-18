using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia.UI {
    public static class WindowExtensions {
        public static void MimicOwnership(this Window window, Window owner) {
            window.Height = owner.Height > 500 ? owner.Height * 0.9 : 500;
            window.Width = owner.Width > 800 ? owner.Width * 0.9 : 800;

            var ownerPoint = owner.PointToScreen(new Point(0, 0));

            var left = ownerPoint.X + (owner.Width / 2) - (window.Width / 2);
            var top = ownerPoint.Y + (owner.Height / 2) - (window.Height / 2);
            window.Left = left < ownerPoint.X ? ownerPoint.X : left;
            window.Top = top < ownerPoint.Y ? ownerPoint.Y : top;
        }
    }
}
