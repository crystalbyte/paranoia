﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.UI {
    public interface ITokenMatcher {
        bool IsMatch(string value);
    }
}
