using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public sealed class MetroNavigationFrame : Control {
        static MetroNavigationFrame() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }
    }
}
