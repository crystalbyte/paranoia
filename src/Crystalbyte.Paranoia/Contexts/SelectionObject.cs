﻿#region Copyright Notice & Copying Permission

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

#endregion

namespace Crystalbyte.Paranoia {
    public abstract class SelectionObject : NotificationObject {
        private bool _isSelected;

        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged() {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
                OnSelectionChanged();
            }
        }
    }
}