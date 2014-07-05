using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia.Tests.Data {

    [TestClass]
    public sealed class SQLiteModelAnalyzerTests {

        [TestMethod]
        public void GetTableCreateScriptTest() {
            var analyzer = new SQLiteModelAnalyzer(typeof(Person));
            var actual = analyzer.GetTableCreateScript();
            const string expected = "CREATE TABLE Person(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER, Residence TEXT, CategoryId INTEGER, FOREIGN KEY(CategoryId) REFERENCES Category(Id));";
            Assert.AreEqual(expected, actual);
        }

        [Table("Category")]
        public class Category {
            [Key]
            public int Id { get; set; }

            public ICollection<Person> Persons { get; set; }
        }

        [Table("Person")]
        public class Person {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }

            [Column("Residence")]
            public string Address { get; set; }

            [ForeignKey("Category")]
            public int CategoryId { get; set; }

            public Category Category { get; set; }
        }
    }
}
