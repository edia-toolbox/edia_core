using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Edia.Data;

namespace Edia.Tests {

    public class TestDataTable {

        [Test]
        public void CreateDataTable() {
            var dt = new DataTable("col1", "col2", "col3");
            Assert.AreEqual(new[] { "col1", "col2", "col3" }, dt.Headers);
            Assert.AreEqual(0, dt.CountRows());
        }

        [Test]
        public void AddCompleteRow() {
            var dt = new DataTable("name", "value");
            var row = new DataRow();
            row.Add(("name", "test"));
            row.Add(("value", 42));
            dt.AddCompleteRow(row);

            Assert.AreEqual(1, dt.CountRows());
        }

        [Test]
        public void AddMultipleRows() {
            var dt = new DataTable("a", "b");
            for (int i = 0; i < 5; i++) {
                var row = new DataRow();
                row.Add(("a", i));
                row.Add(("b", i * 10));
                dt.AddCompleteRow(row);
            }
            Assert.AreEqual(5, dt.CountRows());
        }

        [Test]
        public void AddRowWithWrongColumnsThrows() {
            var dt = new DataTable("a", "b");
            var row = new DataRow();
            row.Add(("x", 1));
            row.Add(("y", 2));

            Assert.Throws<InvalidOperationException>(() => dt.AddCompleteRow(row));
        }

        [Test]
        public void AddRowWithDifferentCountThrows() {
            var dt = new DataTable("a", "b", "c");
            var row = new DataRow();
            row.Add(("a", 1));
            row.Add(("b", 2));

            Assert.Throws<InvalidOperationException>(() => dt.AddCompleteRow(row));
        }

        [Test]
        public void AddNullRowThrows() {
            var dt = new DataTable("a");
            Assert.Throws<ArgumentNullException>(() => dt.AddCompleteRow(null));
        }

        [Test]
        public void GetCSVLines_InvariantCulture() {
            var culture = CultureInfo.InvariantCulture;
            var dt = new DataTable("name", "score");
            var row = new DataRow();
            row.Add(("name", "alice"));
            row.Add(("score", 3.14f));
            dt.AddCompleteRow(row);

            string[] lines = dt.GetCSVLines(culture);
            Assert.AreEqual("name,score", lines[0]);
            Assert.AreEqual("alice,3.14", lines[1]);
        }

        [Test]
        public void GetCSVLines_NullValue() {
            var culture = CultureInfo.InvariantCulture;
            var dt = new DataTable("a", "b");
            var row = new DataRow();
            row.Add(("a", null));
            row.Add(("b", "ok"));
            dt.AddCompleteRow(row);

            string[] lines = dt.GetCSVLines(culture);
            Assert.AreEqual("null,ok", lines[1]);
        }

        [Test]
        public void GetCSVLines_CommasReplaced() {
            var culture = CultureInfo.InvariantCulture;
            var dt = new DataTable("text");
            var row = new DataRow();
            row.Add(("text", "hello, world"));
            dt.AddCompleteRow(row);

            string[] lines = dt.GetCSVLines(culture);
            // Commas in values should be replaced with underscores
            Assert.AreEqual("hello_ world", lines[1]);
        }

        [Test]
        public void GetCSVLines_MultipleRows() {
            var culture = CultureInfo.InvariantCulture;
            var dt = new DataTable("id", "val");

            for (int i = 1; i <= 3; i++) {
                var row = new DataRow();
                row.Add(("id", i));
                row.Add(("val", i * 100));
                dt.AddCompleteRow(row);
            }

            string[] lines = dt.GetCSVLines(culture);
            Assert.AreEqual(4, lines.Length); // 1 header + 3 data rows
            Assert.AreEqual("id,val", lines[0]);
            Assert.AreEqual("1,100", lines[1]);
            Assert.AreEqual("2,200", lines[2]);
            Assert.AreEqual("3,300", lines[3]);
        }

        [Test]
        public void GetCSVLines_FloatFormatting() {
            var culture = CultureInfo.InvariantCulture;
            var dt = new DataTable("f", "d");
            var row = new DataRow();
            row.Add(("f", 3.14f));
            row.Add(("d", 2.71828d));
            dt.AddCompleteRow(row);

            string[] lines = dt.GetCSVLines(culture);
            Assert.AreEqual("3.14,2.71828", lines[1]);
        }

        [Test]
        public void FromCSV() {
            string[] csvLines = new[] {
                "name,age,score",
                "alice,30,95.5",
                "bob,25,88.0"
            };

            DataTable dt = DataTable.FromCSV(csvLines);
            Assert.AreEqual(new[] { "name", "age", "score" }, dt.Headers);
            Assert.AreEqual(2, dt.CountRows());
        }

        [Test]
        public void FromCSV_MismatchedColumnsThrows() {
            string[] csvLines = new[] {
                "a,b,c",
                "1,2"  // missing column
            };

            Assert.Throws<Exception>(() => DataTable.FromCSV(csvLines));
        }

        [Test]
        public void GetAsDictOfList() {
            var dt = new DataTable("x", "y");
            var r1 = new DataRow(); r1.Add(("x", 1)); r1.Add(("y", "a")); dt.AddCompleteRow(r1);
            var r2 = new DataRow(); r2.Add(("x", 2)); r2.Add(("y", "b")); dt.AddCompleteRow(r2);

            var result = dt.GetAsDictOfList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new List<object> { 1, 2 }, result["x"]);
            Assert.AreEqual(new List<object> { "a", "b" }, result["y"]);
        }

        [Test]
        public void GetAsListOfDict() {
            var dt = new DataTable("x", "y");
            var r1 = new DataRow(); r1.Add(("x", 1)); r1.Add(("y", "a")); dt.AddCompleteRow(r1);
            var r2 = new DataRow(); r2.Add(("x", 2)); r2.Add(("y", "b")); dt.AddCompleteRow(r2);

            var result = dt.GetAsListOfDict();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0]["x"]);
            Assert.AreEqual("a", result[0]["y"]);
            Assert.AreEqual(2, result[1]["x"]);
            Assert.AreEqual("b", result[1]["y"]);
        }

        [Test]
        public void EmptyTableCountRows() {
            var dt = new DataTable("a");
            Assert.AreEqual(0, dt.CountRows());
        }

        [Test]
        public void EmptyTableNoHeaders() {
            var dt = new DataTable();
            Assert.AreEqual(0, dt.Headers.Length);
            Assert.AreEqual(0, dt.CountRows());
        }
    }
}
