using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        [Import]
        public static AppContext AppContext { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Compose();
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
