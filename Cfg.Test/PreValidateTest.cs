using System.Collections.Generic;
using Cfg.Net;
using Cfg.Net.Contracts;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class PreValidateTest {

        [Test]
        public void TestPreValidateAProperty() {
            const string resource = @"<cfg>
                <things>
                    <add name='one' value='something' />
                    <add name='two' value='Another' />
                </things>
            </cfg>";
            var actual = new TestProperty(resource, new MyLogger());
            Assert.AreEqual(0, actual.Errors().Length);
            Assert.AreEqual(2, actual.Things.Count);
        }


        [Test]
        public void TestPreValidateACollection() {
            const string resource = @"<cfg>
                <things>
                    <add name='one' value='something' />
                    <add name='two' value='Another' />
                </things>
            </cfg>";
            var actual = new TestCollection(resource, new MyLogger());
            Assert.AreEqual(1, actual.Errors().Length);
            Assert.AreEqual(3, actual.Things.Count);
        }

        private class TestProperty : CfgNode {
            public TestProperty(string cfg, ILogger logger)
                : base(logger: logger) {
                Load(cfg);
            }
            [Cfg]
            public List<Thing> Things { get; set; }
        }

        private class Thing : CfgNode {
            [Cfg]
            public string Name { get; set; }
            [Cfg(domain = "Something,Another", ignoreCase = false)]
            public string Value { get; set; }

            protected override void PreValidate() {
                if (char.IsLower(Value[0])) {
                    Value = char.ToUpper(Value[0]) + Value.Substring(1);
                }
            }
        }

        private class TestCollection : CfgNode {
            public TestCollection(string cfg, ILogger logger)
                : base(logger: logger) {
                Load(cfg);
            }
            [Cfg]
            public List<Thing> Things { get; set; }

            protected override void PreValidate() {
                Things.Add(GetValidatedOf<Thing>(thing => { thing.Name = "three"; thing.Value = "error"; }));
            }
        }

    }
}
