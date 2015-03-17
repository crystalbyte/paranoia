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

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class RichTextBoxExtensions {
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached("BindableText", typeof (string), typeof (RichTextBoxExtensions),
                new UIPropertyMetadata(null, BindableTextPropertyChanged));

        public static string GetBindableText(DependencyObject obj) {
            return (string) obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject obj, string value) {
            obj.SetValue(BindableTextProperty, value);
        }

        public static void BindableTextPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var textbox = o as RichTextBox;
            if (textbox == null) return;

            try {
                textbox.BeginChange();

                var text = e.NewValue as string;
                if (string.IsNullOrEmpty(text)) {
                    if (textbox.CaretPosition.Paragraph != null)
                        textbox.CaretPosition.Paragraph.Inlines.Clear();
                    return;
                }

                if (textbox.CaretPosition.Paragraph == null) {
                    return;
                }

                textbox.CaretPosition.Paragraph.Inlines.Clear();
                textbox.CaretPosition.Paragraph.Inlines.Add(text);
            }
            finally {
                textbox.EndChange();
            }
        }
    }
}