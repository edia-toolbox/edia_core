using NUnit.Framework;
using System.Linq;
using Edia.Data;

namespace Edia.Tests {

    public class TestDataRow {

        [Test]
        public void CreateEmptyRow() {
            var row = new DataRow();
            Assert.AreEqual(0, row.Count);
        }

        [Test]
        public void AddTuples() {
            var row = new DataRow();
            row.Add(("name", "test"));
            row.Add(("value", 42));

            Assert.AreEqual(2, row.Count);
            Assert.AreEqual("name", row[0].columnName);
            Assert.AreEqual("test", row[0].value);
            Assert.AreEqual("value", row[1].columnName);
            Assert.AreEqual(42, row[1].value);
        }

        [Test]
        public void Headers() {
            var row = new DataRow();
            row.Add(("a", 1));
            row.Add(("b", 2));
            row.Add(("c", 3));

            var headers = row.Headers.ToList();
            Assert.AreEqual(3, headers.Count);
            Assert.AreEqual("a", headers[0]);
            Assert.AreEqual("b", headers[1]);
            Assert.AreEqual("c", headers[2]);
        }

        [Test]
        public void NullValue() {
            var row = new DataRow();
            row.Add(("key", null));
            Assert.AreEqual(1, row.Count);
            Assert.IsNull(row[0].value);
        }
    }
}
