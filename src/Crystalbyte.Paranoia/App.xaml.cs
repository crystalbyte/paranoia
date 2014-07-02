#region Using directives

using System.Composition;
using System.Composition.Hosting;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public static readonly string Name = "Paranoia";

        [Import]
        public static Foundation Foundation { get; set; }

        internal CompositionHost Composition { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            Compose();
            base.OnStartup(e);
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof (App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }
    }
}