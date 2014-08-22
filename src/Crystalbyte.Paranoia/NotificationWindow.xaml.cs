#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow {
        private readonly IEnumerable<MailMessageContext> _messages;
        private Storyboard _entryStoryboard;
        private Storyboard _exitStoryboard;
        private readonly AudioPlayer _audioPlayer;

        public NotificationWindow(ICollection<MailMessageContext> messages) {
            _messages = messages;

            var stream = LoadSoundStream();
            _audioPlayer = new AudioPlayer(stream) { Volume = .8f };

            InitializeComponent();
            DataContext = new NotificationWindowContext(messages);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            _entryStoryboard.Begin();
            _audioPlayer.Play();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Left = SystemParameters.WorkArea.Width - Width;
            Top = SystemParameters.WorkArea.Top + 20;

            _entryStoryboard = (Storyboard)Resources["EntryAnimation"];
            _entryStoryboard.Completed += OnEntryStoryboardCompleted;

            _exitStoryboard = (Storyboard)Resources["ExitAnimation"];
            _exitStoryboard.Completed += OnExitAnimationCompleted;
        }

        private static Stream LoadSoundStream() {
            var info = Application.GetResourceStream(new Uri(@"/Assets/c2_please-answer.ogg", UriKind.Relative));
            if (info == null) {
                throw new NullReferenceException("info");
            }
            info.Stream.Seek(0, SeekOrigin.Begin);
            return info.Stream;
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            if (_audioPlayer != null) {
                _audioPlayer.Dispose();
            }
        }

        private void SlideOut() {
            _exitStoryboard.Begin();
        }

        private void OnEntryStoryboardCompleted(object sender, EventArgs e) {
            SlideOut();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            SlideOut();
        }

        private void OnWindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_messages.Any()) {
                return;
            }

            App.Context.ShowMessage(_messages.First());
            SlideOut();
        }



        private void OnExitAnimationCompleted(object sender, EventArgs e) {
            Close();
        }
    }
}