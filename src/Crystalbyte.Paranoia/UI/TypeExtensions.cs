﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public static class TypeExtensions {
        public static Uri ToPageUri(this Type type)  {
            return new Uri(string.Format("/UI/Pages/{0}.xaml", type.Name), UriKind.Relative);
        }
    }
}