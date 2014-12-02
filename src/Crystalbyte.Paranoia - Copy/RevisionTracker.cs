#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Crystalbyte.Paranoia {
    internal sealed class RevisionTracker<T> where T : INotifyPropertyChanged {
        private readonly T _trackedObject;
        private readonly Dictionary<string, Stack<object>> _revisions;

        public RevisionTracker(T trackedObject) {
            _trackedObject = trackedObject;
            _revisions = new Dictionary<string, Stack<object>>();
        }

        private void OnTrackedObjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (!_revisions.ContainsKey(e.PropertyName)) {
                return;
            }

            TakeSnapshot(e.PropertyName);
        }

        private void TakeSnapshot(string propertyName) {
            var value = typeof (T).GetProperty(propertyName).GetValue(_trackedObject);
            var stack = _revisions[propertyName];
            stack.Push(value);
        }

        internal RevisionTracker<T> WithProperty<TReturn>(Expression<Func<T, TReturn>> expression) {
            var propertyName = PropertySupport.ExtractPropertyName(expression);
            _revisions.Add(propertyName, new Stack<object>());
            TakeSnapshot(propertyName);
            return this;
        }

        internal RevisionTracker<T> Start() {
            _trackedObject.PropertyChanged += OnTrackedObjectPropertyChanged;
            return this;
        }


        internal void Revert() {
            foreach (var revision in _revisions) {
                var stack = revision.Value;
                var initialValue = stack.Last();
                typeof (T).GetProperty(revision.Key).SetValue(_trackedObject, initialValue);
            }
        }

        internal void Reset() {
            foreach (var revision in _revisions) {
                var stack = revision.Value;
                stack.Clear();
                TakeSnapshot(revision.Key);
            }
        }

        internal RevisionTracker<T> Stop() {
            _trackedObject.PropertyChanged -= OnTrackedObjectPropertyChanged;
            return this;
        }
    }
}