#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
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