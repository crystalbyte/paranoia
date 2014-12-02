using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Data;
using Microsoft.Win32;
using NLog;

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class IconFinder : IValueConverter {

        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string DefaultIcon = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                var isLarge = true;
                var s = parameter as string;
                if (!string.IsNullOrEmpty(s) || string.Compare(s, "16", StringComparison.InvariantCultureIgnoreCase) == 0) {
                    isLarge = false;
                }
                var name = value as string;
                if (string.IsNullOrEmpty(name)) {
                    return DefaultIcon;
                }

                var extension = name.Split('.').LastOrDefault();
                if (string.IsNullOrEmpty(extension)) {
                    return DefaultIcon;
                }

                var location = GetFileTypeAndIcon(extension);
                if (string.IsNullOrEmpty(location)) {
                    return DefaultIcon;
                }

                var icon = ExtractIconFromFile(location, isLarge);
                if (icon == null) {
                    return DefaultIcon;
                }

                var source = icon.ToImageSource();
                NativeMethods.DestroyIcon(icon.Handle);
                return source;
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }

            return DefaultIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        public static string GetFileTypeAndIcon(string extension) {
            using (var root = Registry.ClassesRoot) {
                if (string.IsNullOrEmpty(extension)) {
                    return DefaultIcon;
                }

                var key = root.OpenSubKey(string.Format(".{0}", extension)) ?? root.OpenSubKey("*");
                if (key == null) {
                    return DefaultIcon;
                }
                var value = key.GetValue("");
                key.Close();

                var icon = root.OpenSubKey(string.Format("{0}\\DefaultIcon", value));
                if (icon == null) {
                    return DefaultIcon;
                }

                var location = icon.GetValue("");
                icon.Close();

                if (location != null) {
                    return location.ToString().Replace("\"", "");
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///     Extracts an icon from a file.
        /// </summary>
        /// <param name="location">
        ///     The params string, such as: "C:\\Program Files\\NetMeeting\\conf.exe,1".</param>
        /// <param name="isLarge">
        ///     Determines the returned icon is a large (may be 32x32 px) 
        ///     or small icon (16x16 px).
        /// </param>
        /// <returns>
        ///     Return an instance of <see cref="System.Drawing.Icon"/> or null.
        /// </returns>
        public static Icon ExtractIconFromFile(string location, bool isLarge) {

            var values = location.Split(',');
            if (values.Length != 2) {
                return null;
            }

            var name = values.First();
            var index = int.Parse(values.Last());

            var handles = new[] {IntPtr.Zero};

            var success = isLarge
                ? NativeMethods.ExtractIconEx(name, index, handles, null, 1)
                : NativeMethods.ExtractIconEx(name, index, null, handles, 1);

            return success > 0 ? Icon.FromHandle(handles[0]) : null;
        }

        private static class NativeMethods {
            [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

            [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
            public static extern int DestroyIcon(IntPtr hIcon);
        }
    }
}
