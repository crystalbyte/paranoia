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
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class CreateMailboxContext : NotificationObject {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMailboxCreator _creator;
        private string _name;

        #endregion

        #region Construction

        public CreateMailboxContext(IMailboxCreator creator) {
            _creator = creator;
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
            get { return !string.IsNullOrWhiteSpace(Name) && _creator.CheckForValidName(Name); }
        }

        internal async Task CommitAsync() {
            try {
                await _creator.CreateMailboxAsync(Name);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}