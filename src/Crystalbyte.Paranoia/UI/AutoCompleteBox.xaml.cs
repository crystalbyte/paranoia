using System;

using System.Reactive.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Concurrency;
using System.Threading;

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = InputControlPartName, Type = typeof(TextBox))]
    public sealed class AutoCompleteBox : TextBox {

        #region Xaml Support

        private const string InputControlPartName = "PART_InputControl";

        #endregion

        #region Private Fields

        private TextBox _inputControl;

        #endregion

        static AutoCompleteBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                action => TextChanged += action,
                action => TextChanged -= action)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(SynchronizationContext.Current)
            .Select(x => ((TextBox)x.Sender).Text)
            .Subscribe(OnTextChangeConfirmed);
        }

        private void OnTextChangeConfirmed(string text) {

        }
    }
}
