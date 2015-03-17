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
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class CompositionContext : SelectionObject {
        private readonly CompositionModel _model;

        public CompositionContext(CompositionModel model) {
            _model = model;
        }

        public bool IsSubjectNilOrEmpty {
            get { return string.IsNullOrEmpty(Subject); }
        }

        public string ToNameOrAddress {
            get { return string.IsNullOrEmpty(ToName) ? ToAddress : ToName; }
        }

        public DateTime Date {
            get { return _model.Date; }
        }

        public string Subject {
            get { return _model.Subject; }
        }

        public string ToName {
            get { return _model.ToName; }
        }

        public string ToAddress {
            get { return _model.ToAddress; }
        }

        public Int64 Id {
            get { return _model.Id; }
        }
    }
}