#region Using directives

using System;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Data {
    internal sealed class SQLiteConnectionStringReader {
        private readonly string _connectionString;

        public SQLiteConnectionStringReader(string connectionString) {
            _connectionString = connectionString;
        }

        public string DataSource {
            get {
                var match = Regex.Match(_connectionString, "Data Source=.+?;", RegexOptions.IgnoreCase);
                if (!match.Success) {
                    return string.Empty;
                }

                var value = match.Value.Split('=')[1].TrimEnd(';');
                var directory = (string) AppDomain.CurrentDomain.GetData("DataDirectory");
                return Regex.Replace(value, @"\|DataDirectory\|", x => directory);
            }
        }
    }
}