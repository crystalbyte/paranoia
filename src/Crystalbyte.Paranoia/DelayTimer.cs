using System;
using System.Windows.Threading;

namespace Crystalbyte.Paranoia {
    public sealed class DelayTimer {
        #region Private Fields

        private readonly DispatcherTimer _timer;

        #endregion

        #region Public Events

        public event EventHandler TimerElapsed;

        #endregion

        #region Construction

        public DelayTimer() {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += (sender, e) => {
                _timer.Stop();
                OnTimerElapsed();
            };
        }

        #endregion

        public TimeSpan Interval {
            set { _timer.Interval = value; }
        }

        private void OnTimerElapsed() {
            var handler = TimerElapsed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public void Touch() {
            _timer.Stop();
            _timer.Start();
        }
    }
}
