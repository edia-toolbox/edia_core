using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Edia.Data;

namespace Edia.Tests {

    public class TestResultsDictionary {

        [Test]
        public void CreateWithInitialKeys() {
            var keys = new[] { "a", "b", "c" };
            var rd = new ResultsDictionary(keys, false);

            Assert.IsTrue(rd.ContainsKey("a"));
            Assert.IsTrue(rd.ContainsKey("b"));
            Assert.IsTrue(rd.ContainsKey("c"));
            Assert.IsFalse(rd.ContainsKey("d"));
        }

        [Test]
        public void InitialValuesAreEmptyString() {
            var rd = new ResultsDictionary(new[] { "key" }, false);
            Assert.AreEqual(string.Empty, rd["key"]);
        }

        [Test]
        public void SetAndGetValue() {
            var rd = new ResultsDictionary(new[] { "score" }, false);
            rd["score"] = 42;
            Assert.AreEqual(42, rd["score"]);
        }

        [Test]
        public void OverwriteExistingKey() {
            var rd = new ResultsDictionary(new[] { "key" }, false);
            rd["key"] = "first";
            rd["key"] = "second";
            Assert.AreEqual("second", rd["key"]);
        }

        [Test]
        public void AdHocAddingDisabled_ThrowsOnNewKey() {
            var rd = new ResultsDictionary(new[] { "existing" }, false);
            Assert.Throws<KeyNotFoundException>(() => rd["new_key"] = "value");
        }

        [Test]
        public void AdHocAddingEnabled_AllowsNewKey() {
            var rd = new ResultsDictionary(new[] { "existing" }, true);
            rd["new_key"] = "value";
            Assert.AreEqual("value", rd["new_key"]);
        }

        [Test]
        public void Keys_ReturnsAllKeys() {
            var rd = new ResultsDictionary(new[] { "a", "b" }, true);
            rd["c"] = "added";

            var keys = rd.Keys;
            Assert.IsTrue(keys.Contains("a"));
            Assert.IsTrue(keys.Contains("b"));
            Assert.IsTrue(keys.Contains("c"));
        }

        [Test]
        public void ContainsKey_FalseForMissing() {
            var rd = new ResultsDictionary(new string[] { }, true);
            Assert.IsFalse(rd.ContainsKey("nothing"));
        }

        [Test]
        public void NullValue() {
            var rd = new ResultsDictionary(new[] { "key" }, false);
            rd["key"] = null;
            Assert.IsNull(rd["key"]);
        }

        [Test]
        public void MultipleAdHocKeys() {
            var rd = new ResultsDictionary(new string[] { }, true);
            for (int i = 0; i < 100; i++) {
                rd[$"key_{i}"] = i;
            }
            Assert.AreEqual(100, rd.Keys.Count);
            Assert.AreEqual(50, rd["key_50"]);
        }
    }
}
