using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class SQLiteModelAnalyzer {
        private readonly Type _type;

        public SQLiteModelAnalyzer(Type type) {
            _type = type;
        }

        public string GetTableCreateScript() {
            var tableName = _type.GetCustomAttribute<TableAttribute>().Name;
            var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var key = properties.FirstOrDefault(x => x.GetCustomAttribute<KeyAttribute>() != null);
            if (key == null) {
                var message = string.Format(Resources.MissingKeyExceptionTemplate, _type.Name);
                throw new Exception(message);
            }

            using (var writer = new StringWriter()) {
                writer.Write("CREATE TABLE ");
                writer.Write(tableName);
                writer.Write("(");
                http://sqlite.awardspace.info/syntax/sqlitedata.htm
                throw new NotImplementedException();
                writer.Write(")");
            }

            

            return string.Empty;
        }
    }
}
