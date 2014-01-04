﻿#region Using directives

using Crystalbyte.Paranoia.Contexts;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountScreen.xaml
    /// </summary>
    public partial class CreateAccountScreen {
        public CreateAccountScreen() {
            ScreenContext = App.AppContext.CreateAccountScreenContext;
            InitializeComponent();
        }

        public CreateAccountScreenContext ScreenContext {
            get { return DataContext as CreateAccountScreenContext; }
            set { DataContext = value; }
        }
    }
}