using NUnit.Framework;
using System;
using System.Collections.Generic;
using Edia.Data;

namespace Edia.Tests {

    public class TestSettings {

        [Test]
        public void CreateEmptySettings() {
            Settings settings = Settings.empty;
            Assert.IsNotNull(settings);
            Assert.AreEqual(0, settings.Keys.Count);
        }

        [Test]
        public void SetAndGetString() {
            Settings settings = Settings.empty;
            settings.SetValue("name", "hello world");
            Assert.AreEqual("hello world", settings.GetString("name"));
        }

        [Test]
        public void SetAndGetInt() {
            Settings settings = Settings.empty;
            settings.SetValue("count", 42);
            Assert.AreEqual(42, settings.GetInt("count"));
        }

        [Test]
        public void SetAndGetFloat() {
            Settings settings = Settings.empty;
            settings.SetValue("pi", 3.14f);
            Assert.AreEqual(3.14f, settings.GetFloat("pi"));
        }

        [Test]
        public void SetAndGetDouble() {
            Settings settings = Settings.empty;
            settings.SetValue("pi", 3.1415926d);
            Assert.AreEqual(3.1415926d, settings.GetDouble("pi"));
        }

        [Test]
        public void SetAndGetLong() {
            Settings settings = Settings.empty;
            settings.SetValue("big", 65536L);
            Assert.AreEqual(65536L, settings.GetLong("big"));
        }

        [Test]
        public void SetAndGetBool() {
            Settings settings = Settings.empty;
            settings.SetValue("flag", true);
            Assert.AreEqual(true, settings.GetBool("flag"));
        }

        [Test]
        public void SetAndGetNull() {
            Settings settings = Settings.empty;
            settings.SetValue("nothing", null);
            Assert.IsNull(settings.GetObject("nothing"));
        }

        [Test]
        public void GetMissingKeyThrows() {
            Settings settings = Settings.empty;
            Assert.Throws<KeyNotFoundException>(() => settings.GetObject("missing"));
        }

        [Test]
        public void DictToSettings() {
            var dict = new Dictionary<string, object> {
                { "str", "hello" },
                { "num", 256 },
                { "flag", true },
                { "pi", 3.14f },
                { "nothing", null }
            };
            Settings settings = new Settings(dict);

            Assert.AreEqual("hello", settings.GetString("str"));
            Assert.AreEqual(256, settings.GetInt("num"));
            Assert.AreEqual(true, settings.GetBool("flag"));
            Assert.AreEqual(3.14f, settings.GetFloat("pi"));
            Assert.IsNull(settings.GetObject("nothing"));
        }

        [Test]
        public void GetDict() {
            var inner = new Dictionary<string, object> {
                { "key1", "value1" },
                { "key2", 256 }
            };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "obj", inner }
            });

            Assert.AreEqual(inner, settings.GetDict("obj"));
        }

        [Test]
        public void SetValueOverwrites() {
            Settings settings = Settings.empty;
            settings.SetValue("key", "first");
            settings.SetValue("key", "second");
            Assert.AreEqual("second", settings.GetString("key"));
        }

        [Test]
        public void UpdateWithDict() {
            Settings settings = new Settings(new Dictionary<string, object> {
                { "a", 1 },
                { "b", 2 }
            });

            settings.UpdateWithDict(new Dictionary<string, object> {
                { "b", 20 },
                { "c", 30 }
            });

            Assert.AreEqual(1, settings.GetInt("a"));
            Assert.AreEqual(20, settings.GetInt("b"));
            Assert.AreEqual(30, settings.GetInt("c"));
        }

        [Test]
        public void ContainsKey() {
            Settings settings = new Settings(new Dictionary<string, object> {
                { "exists", 1 }
            });

            Assert.IsTrue(settings.ContainsKey("exists"));
            Assert.IsFalse(settings.ContainsKey("missing"));
        }

        // --- Cascading ---

        [Test]
        public void CascadeToParent() {
            Settings parent = new Settings(new Dictionary<string, object> {
                { "parent_key", "parent_value" }
            });

            Settings child = Settings.empty;
            child.SetParent(parent);

            Assert.AreEqual("parent_value", child.GetString("parent_key"));
        }

        [Test]
        public void ChildOverridesParent() {
            Settings parent = new Settings(new Dictionary<string, object> {
                { "shared", "parent" }
            });

            Settings child = new Settings(new Dictionary<string, object> {
                { "shared", "child" }
            });
            child.SetParent(parent);

            Assert.AreEqual("child", child.GetString("shared"));
        }

        [Test]
        public void CascadeThreeLevels() {
            Settings grandparent = new Settings(new Dictionary<string, object> {
                { "level", "grandparent" },
                { "gp_only", "gp_value" }
            });

            Settings parent = Settings.empty;
            parent.SetParent(grandparent);
            parent.SetValue("level", "parent");

            Settings child = Settings.empty;
            child.SetParent(parent);

            // child has no "level" - cascades to parent
            Assert.AreEqual("parent", child.GetString("level"));
            // "gp_only" cascades through parent to grandparent
            Assert.AreEqual("gp_value", child.GetString("gp_only"));
        }

        [Test]
        public void CascadeThrowsWhenNotFoundAnywhere() {
            Settings parent = Settings.empty;
            Settings child = Settings.empty;
            child.SetParent(parent);

            Assert.Throws<KeyNotFoundException>(() => child.GetObject("missing"));
        }

        [Test]
        public void ContainsKeyCascades() {
            Settings parent = new Settings(new Dictionary<string, object> {
                { "parent_key", "value" }
            });

            Settings child = Settings.empty;
            child.SetParent(parent);

            Assert.IsTrue(child.ContainsKey("parent_key"));
            Assert.IsFalse(child.ContainsKey("unknown"));
        }

        [Test]
        public void ChildDoesNotAffectParent() {
            Settings parent = Settings.empty;
            Settings child = Settings.empty;
            child.SetParent(parent);

            child.SetValue("child_only", "value");
            Assert.Throws<KeyNotFoundException>(() => parent.GetObject("child_only"));
        }

        // --- List getters ---

        [Test]
        public void GetObjectList() {
            var list = new List<object> { 1, "two", 3.0 };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(list, settings.GetObjectList("list"));
        }

        [Test]
        public void GetIntListFromObjectList() {
            var list = new List<object> { 1, 2, 3 };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(new List<int> { 1, 2, 3 }, settings.GetIntList("list"));
        }

        [Test]
        public void GetFloatListFromObjectList() {
            var list = new List<object> { 1.5, 2.5, 3.5 };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(new List<float> { 1.5f, 2.5f, 3.5f }, settings.GetFloatList("list"));
        }

        [Test]
        public void GetDoubleListFromObjectList() {
            var list = new List<object> { 1.44, 2.44, 3.44 };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(new List<double> { 1.44, 2.44, 3.44 }, settings.GetDoubleList("list"));
        }

        [Test]
        public void GetStringListFromObjectList() {
            var list = new List<object> { "a", "b", "c" };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(new List<string> { "a", "b", "c" }, settings.GetStringList("list"));
        }

        [Test]
        public void GetBoolListFromObjectList() {
            var list = new List<object> { true, false, true };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(new List<bool> { true, false, true }, settings.GetBoolList("list"));
        }

        [Test]
        public void CastNativeIntList() {
            var list = new List<int> { 10, 20, 30 };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(list, settings.GetIntList("list"));
        }

        [Test]
        public void CastNativeFloatList() {
            var list = new List<float> { 1.1f, 2.2f };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(list, settings.GetFloatList("list"));
        }

        [Test]
        public void CastNativeBoolList() {
            var list = new List<bool> { false, true, false };
            Settings settings = new Settings(new Dictionary<string, object> {
                { "list", list }
            });
            Assert.AreEqual(list, settings.GetBoolList("list"));
        }

        // --- Default-value overloads ---

        [Test]
        public void GetStringWithDefault_KeyExists() {
            Settings settings = new Settings(new Dictionary<string, object> {
                { "key", "value" }
            });
            Assert.AreEqual("value", settings.GetString("key", "default"));
        }

        [Test]
        public void GetStringWithDefault_KeyMissing() {
            Settings settings = Settings.empty;
            Assert.AreEqual("default", settings.GetString("missing", "default"));
        }

        [Test]
        public void GetObjectWithDefault_KeyMissing() {
            Settings settings = Settings.empty;
            Assert.AreEqual("fallback", settings.GetObject("missing", "fallback"));
        }

        [Test]
        public void GetIntWithDefault_KeyMissing() {
            Settings settings = Settings.empty;
            Assert.AreEqual(99, settings.GetInt("missing", 99));
        }

        [Test]
        public void GetBoolWithDefault_KeyMissing() {
            Settings settings = Settings.empty;
            Assert.AreEqual(true, settings.GetBool("missing", true));
        }

        [Test]
        public void GetDefaultCascadesToParent() {
            Settings parent = new Settings(new Dictionary<string, object> {
                { "key", "parent_value" }
            });
            Settings child = Settings.empty;
            child.SetParent(parent);

            // Should find in parent, not use default
            Assert.AreEqual("parent_value", child.GetString("key", "default"));
        }

        [Test]
        public void GetDefaultUsedWhenNotInParentEither() {
            Settings parent = Settings.empty;
            Settings child = Settings.empty;
            child.SetParent(parent);

            Assert.AreEqual("default", child.GetObject("nope", "default"));
        }
    }
}
