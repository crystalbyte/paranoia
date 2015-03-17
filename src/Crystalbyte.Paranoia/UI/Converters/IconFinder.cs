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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using NLog;
using Point = System.Drawing.Point;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class IconFinder : IValueConverter {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string DefaultIcon = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                var name = value as string;
                if (string.IsNullOrEmpty(name)) {
                    return DefaultIcon;
                }

                var extension = name.Split('.').LastOrDefault();
                if (string.IsNullOrEmpty(extension)) {
                    return DefaultIcon;
                }

                return FindLargeFromPath(name, true, false);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }

            return DefaultIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        private static BitmapSource FindFromSource(Icon ic) {
            var ic2 = Imaging.CreateBitmapSourceFromHIcon(ic.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            ic2.Freeze();
            return ic2;
        }

        public static BitmapSource FindLargeFromPath(string fileName, bool jumbo, bool checkDisk) {
            var shinfo = new SHFILEINFO();

            // ReSharper disable InconsistentNaming
            const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

            const uint SHGFI_SYSICONINDEX = 0x4000;
            const int FILE_ATTRIBUTE_NORMAL = 0x80;
            // ReSharper restore InconsistentNaming
            var flags = SHGFI_SYSICONINDEX;

            if (!checkDisk) // This does not seem to work. If I try it, a folder icon is always returned.
                flags |= SHGFI_USEFILEATTRIBUTES;

            var info = new FileInfo(fileName);
            if (!info.Exists) {
                fileName = string.Format("{0}", info.Extension);
            }

            var res = NativeMethods.SHGetFileInfo(fileName, FILE_ATTRIBUTE_NORMAL, ref shinfo, Marshal.SizeOf(shinfo),
                flags);

            if (res == 0)
                throw (new FileNotFoundException());

            var iconIndex = shinfo.iIcon;

            // Get the System IImageList object from the Shell:
            var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

            IImageList iml;
            var size = jumbo ? SHIL_JUMBO : SHIL_EXTRALARGE;
            NativeMethods.SHGetImageList(size, ref iidImageList, out iml);
            var hIcon = IntPtr.Zero;
            // ReSharper disable once InconsistentNaming
            const int ILD_TRANSPARENT = 1;
            iml.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);

            var myIcon = Icon.FromHandle(hIcon);
            var bs = FindFromSource(myIcon);

            myIcon.Dispose();
            bs.Freeze(); // very important to avoid memory leak
            NativeMethods.DestroyIcon(hIcon);
            NativeMethods.SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

            return bs;
        }

        #region IImageList COM Interop (XP)

        [ComImport]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IImageList {
            [PreserveSig]
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

            [PreserveSig]
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);

            [PreserveSig]
            int SetOverlayImage(int iImage, int iOverlay);

            [PreserveSig]
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

            [PreserveSig]
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(int i);

            [PreserveSig]
            int GetIcon(int i, int flags, ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

            [PreserveSig]
            int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int Clone(ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(int i, ref RECT prc);

            [PreserveSig]
            int GetIconSize(ref int cx, ref int cy);

            [PreserveSig]
            int SetIconSize(int cx, int cy);

            [PreserveSig]
            int GetImageCount(ref int pi);

            [PreserveSig]
            int SetImageCount(int uNewCount);

            [PreserveSig]
            int SetBkColor(int clrBk,
                ref int pclr);

            [PreserveSig]
            int GetBkColor(ref int pclr);

            [PreserveSig]
            int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(IntPtr hwndLock, int x, int y);

            [PreserveSig]
            int DragLeave(IntPtr hwndLock);

            [PreserveSig]
            int DragMove(int x, int y);

            [PreserveSig]
            int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

            [PreserveSig]
            int DragShowNolock(int fShow);

            [PreserveSig]
            int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(int i, ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(int iOverlay, ref int piIndex);
        };

        #endregion

        #region Native Support

        // ReSharper disable InconsistentNaming

        private const int SHIL_EXTRALARGE = 0x2;
        private const int SHIL_JUMBO = 0x4;
        private const int WM_CLOSE = 0x0010;

        public struct SHFILEINFO {
            // Handle to the icon representing the file

            public IntPtr hIcon;

            // Index of the icon within the image list

            public int iIcon;

            // Various attributes of the file

            public uint dwAttributes;

            // Path to the file

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;

            // File type

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        };

#pragma warning disable 0649

        // ReSharper disable UnusedField.Compiler
        private struct IMAGELISTDRAWPARAMS {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap; // x offest from the upperleft of bitmap
            public int yBitmap; // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

#pragma warning restore 0649

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT {
            private readonly int _Left;
            private readonly int _Top;
            private readonly int _Right;
            private readonly int _Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            private readonly int X;
            private readonly int Y;

            private POINT(int x, int y) {
                X = x;
                Y = y;
            }

            public static implicit operator Point(POINT p) {
                return new Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p) {
                return new POINT(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGEINFO {
            private readonly IntPtr hbmImage;
            private readonly IntPtr hbmMask;
            private readonly int Unused1;
            private readonly int Unused2;
            private readonly RECT rcImage;
        }

        // ReSharper restore UnusedField.Compiler
        // ReSharper restore InconsistentNaming

        private static class NativeMethods {
            [DllImport("user32")]
            public static extern IntPtr SendMessage(IntPtr handle, int msg, IntPtr wParam, IntPtr lParam);

            /// SHGetImageList is not exported correctly in XP.  See KB316931
            /// http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
            /// Apparently (and hopefully) ordinal 727 isn't going to change.
            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

            // The signature of SHGetFileInfo (located in Shell32.dll)
            [DllImport("Shell32.dll")]
            public static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi,
                int cbFileInfo,
                uint uFlags);

            [DllImport("user32")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }

        #endregion
    }
}