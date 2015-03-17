#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Tests
// 
// Crystalbyte.Paranoia.Tests is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Crystalbyte.Paranoia.Tests.Data {
    [TestClass]
    public sealed class SqLiteModelAnalyzerTests {
        [TestMethod]
        public void GetTableCreateScriptTest() {
            var analyzer = new SQLiteModelAnalyzer(typeof (Person));
            var actual = analyzer.GetTableCreateScript();
            const string expected =
                "CREATE TABLE Person(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER, Residence TEXT, CategoryId INTEGER, FOREIGN KEY(CategoryId) REFERENCES Category(Id));";
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