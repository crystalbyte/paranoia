#region Using directives

using System;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public static readonly string Name = "Paranoia";

        [Import]
        public static AppContext Foundation { get; set; }

        internal static CompositionHost Composition { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            Compose();
            InitEnvironment();
        }

        private static void InitEnvironment() {
            var location = ConfigurationManager.AppSettings["DataDirectory"];
            if (string.IsNullOrEmpty(location)) {
                throw new Exception("Entry for the DataDirectory missing from configuration file.");
            }

            var directory = Environment.ExpandEnvironmentVariables(location);
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);

            using (var context = new StorageContext()) {
                var users = context.Users.FirstOrDefault(x => x.Name == "Alexander");
            }
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }
    }
}