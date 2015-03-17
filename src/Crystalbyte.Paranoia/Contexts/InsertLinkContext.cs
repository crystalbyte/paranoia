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
using System.Threading.Tasks;
using Crystalbyte.Paranoia.UI;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class InsertLinkContext : NotificationObject {
        private readonly HtmlEditor _editor;

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _name;

        #endregion

        #region Construction

        public InsertLinkContext(HtmlEditor editor) {
            _editor = editor;
        }

        #endregion

        #region Property Declarations

        public string Name {
            get { return _name; }
            set {
                if (_name == value) {
                    return;
                }
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        #endregion

        #region Method Declarations

        public bool IsValid {
            get { return Uri.IsWellFormedUriString(Name, UriKind.Absolute); }
        }

        internal async Task CommitAsync() {
            try {
                await _editor.SetLinkAsync(Name);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}