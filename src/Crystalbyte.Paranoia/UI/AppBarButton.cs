using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Crystalbyte.Paranoia.Commands;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Crystalbyte.Paranoia.Themes"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Crystalbyte.Paranoia.Themes;assembly=Crystalbyte.Paranoia.Themes"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:AppBarButton/>
    ///
    /// </summary>
    [TemplatePart(Name = OuterBorderTemplateName)]
    public sealed class AppBarButton : Control {

        #region Private Fields

        private const string OuterBorderTemplateName = "PART_OuterBorder";
        private Border _outerBorder;

        #endregion

        #region Construction

        static AppBarButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarButton), new FrameworkPropertyMetadata(typeof(AppBarButton)));
        }

        #endregion

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _outerBorder = (Border)Template.FindName(OuterBorderTemplateName, this);
            _outerBorder.MouseLeftButtonDown += OnLeftMouseButtonDown;
            _outerBorder.MouseLeftButtonUp += OnLeftMouseButtonUp;
        }

        private void OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e) {
            IsMouseDown = false;
            if (Command != null) {
                Command.Execute(null);
            }
        }

        private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e) {
            IsMouseDown = true;
        }

        public bool IsMouseDown {
            get { return (bool)GetValue(IsMouseDownProperty); }
            set { SetValue(IsMouseDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMouseDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMouseDownProperty =
            DependencyProperty.Register("IsMouseDown", typeof(bool), typeof(AppBarButton), new PropertyMetadata(false));

        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(AppBarButton), new PropertyMetadata(null));

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AppBarButton), new PropertyMetadata(string.Empty));

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(AppBarButton), new PropertyMetadata(new NullCommand(), OnCommandPropertyChanged));

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var button = d as AppBarButton;
            if (button == null) {
                return;
            }

            if (e.OldValue != null) {
                button.Decouple(e.OldValue as ICommand);    
            }

            if (e.NewValue != null) {
                button.Couple(e.NewValue as ICommand);
            }
        }

        private void Couple(ICommand command) {
            command.CanExecuteChanged += OnCommandCanExecuteChanged;
        }

        private void Decouple(ICommand command) {
            command.CanExecuteChanged -= OnCommandCanExecuteChanged;
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e) {
            IsEnabled = Command.CanExecute(null);
        }
    }
}
