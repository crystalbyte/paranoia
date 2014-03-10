#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Cryptography;
using System.ComponentModel;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        #region Construction

        public MainWindow() {
            DataContext = App.AppContext;
        }

        #endregion

        #region Class Overrides

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {

                HookEntropyGenerator();
            } catch (Exception) {
                // TODO: We are probably offline or hit the quota, deal with it.
                throw;
            }
        }

        private void HookEntropyGenerator() {
            var helper = new WindowInteropHelper(this);
            //_source = HwndSource.FromHwnd(helper.Handle);
            //if (_source == null) {
            //    throw new NullReferenceException("HwndSource must not be null.");
            //}
            //_source.AddHook(WndProc);
        }

        //private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        //    if (!OpenSslRandom.IsSeededSufficiently) {
        //        OpenSslRandom.AddEntropyFromEvents(msg, wParam, lParam);
        //    }
        //    return IntPtr.Zero;
        //}

        //private void OnMailSelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var context = DataContext as AppContext;
        //    if (context == null) {
        //        return;
        //    }

        //    var list = sender as ListView;
        //    if (list == null) {
        //        return;
        //    }

        //    context.MailSelectionSource.Mails.Clear();
        //    context.MailSelectionSource.Mails.AddRange(list.SelectedItems.OfType<MailContext>());
        //}

        //private void OnIdentitySelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var context = DataContext as AppContext;
        //    if (context == null) {
        //        return;
        //    }

        //    context.IdentitySelectionSource.Identity = e.AddedItems.OfType<IdentityContext>().FirstOrDefault();
        //}

        //private void OnContactSelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var context = DataContext as AppContext;
        //    if (context == null) {
        //        return;
        //    }

        //    context.ContactSelectionSource.Contact = e.AddedItems.OfType<ContactContext>().FirstOrDefault();
        //}

        //private void OnMailboxSelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var context = DataContext as AppContext;
        //    if (context == null) {
        //        return;
        //    }

        //    var mails = context.MailSelectionSource.Mails.ToArray();
        //    mails.ForEach(x => x.IsSelected = false);

        //    context.MailSelectionSource.Mails.Clear();
        //    context.MailboxSelectionSource.Mailbox = e.AddedItems.OfType<MailboxContext>().FirstOrDefault();
        //}
    }
}