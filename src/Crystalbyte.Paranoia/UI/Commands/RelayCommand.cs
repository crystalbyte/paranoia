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
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class RelayCommand : ICommand {
        #region Private Fields

        private readonly Action<object> _onExecute;
        private readonly Predicate<object> _onCanExecute;

        #endregion

        #region Construction

        public RelayCommand(Action<object> onExecute) {
            _onExecute = onExecute;
        }

        public RelayCommand(Predicate<object> onCanExecute, Action<object> onExecute)
            : this(onExecute) {
            _onCanExecute = onCanExecute;
        }

        #endregion

        #region Event Declarations

        public event EventHandler CanExecuteChanged;

        internal void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        public Predicate<object> OnCanExecute {
            get { return _onCanExecute; }
        }

        public Action<object> OnExecute {
            get { return _onExecute; }
        }

        public bool CanExecute(object parameter) {
            return _onCanExecute == null || _onCanExecute(parameter);
        }

        public void Execute(object parameter) {
            if (_onExecute == null) {
                return;
            }
            _onExecute(parameter);
        }
    }
}