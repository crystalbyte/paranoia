#region Using directives

using System.Composition;
using System.Composition.Hosting;
using System.Windows;
using Crystalbyte.Paranoia.Contexts;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        [Import]
        public static AppContext AppContext { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Compose();
            AppContext.RunAsync();
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof (App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }

        public CompositionHost Composition { get; set; }
    }
}