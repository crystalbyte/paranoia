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
using System.Linq;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailContactContext : SelectionObject {

        #region Private Fields

        private bool _hasKeys;
        private bool _isVerified;
        private readonly MailContact _contact;

        #endregion

        internal MailContactContext(MailContact contact) {
            if (contact == null) {
                throw new ArgumentNullException("contact");
            }

            _contact = contact;
        }

        public long Id {
            get { return _contact.Id; }
        }

        public bool IsExternalContentAllowed {
            get { return _contact.IsExternalContentAllowed; }
            set {
                if (_contact.IsExternalContentAllowed == value) {
                    return;
                }

                _contact.IsExternalContentAllowed = value;
                RaisePropertyChanged(() => IsExternalContentAllowed);
            }
        }

        public Authenticity Authenticity {
            get { return _contact.Authenticity; }
            set {
                if (_contact.Authenticity == value) {
                    return;
                }

                _contact.Authenticity = value;
                RaisePropertyChanged(() => Authenticity);
            }
        }

        public SecurityMeasure Security {
            get {
                if (HasKeys && IsVerified) {
                    return SecurityMeasure.EncryptedAndVerified;
                }

                return HasKeys ? SecurityMeasure.Encrypted : SecurityMeasure.None;
            }
        }

        public char Initial {
            get {
                var isEmpty = string.IsNullOrWhiteSpace(Name)
                              || string.Compare(Name, "nil", StringComparison.InvariantCultureIgnoreCase) == 0;
                return isEmpty ? '#' : char.ToUpper(Name.First());
            }
        }

        public bool HasKeys {
            get { return _hasKeys; }
            set {
                if (_hasKeys == value) {
                    return;
                }
                _hasKeys = value;
                RaisePropertyChanged(() => HasKeys);
            }
        }

        public bool IsVerified {
            get { return _isVerified; }
            set {
                if (_isVerified == value) {
                    return;
                }
                _isVerified = value;
                RaisePropertyChanged(() => IsVerified);
            }
        }

        public string Address {
            get { return _contact.Address; }
            set {
                if (_contact.Address == value) {
                    return;
                }

                _contact.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public string Name {
            get { return _contact.Name; }
            set {
                if (_contact.Name == value) {
                    return;
                }

                _contact.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string NameOrAddress {
            get { return string.IsNullOrWhiteSpace(Name) || Name.EqualsIgnoreCase("NIL") ? Address : Name; }
        }
    }
}