using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Shorthand;

namespace Cfg.Test.Shorthand {
    [TestFixture]
    public class ShorthandTest {

        [Test]
        public void TestShorthandConfiguration() {
            var root = new ShorthandRoot(File.ReadAllText(@"shorthand.xml"));

            foreach (var error in root.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, root.Errors().Count());
            Assert.AreEqual(2, root.Signatures.Count);
            Assert.AreEqual(1, root.Targets.Count);
            Assert.AreEqual(4, root.Methods.Count);
        }

        [Test]
        public void TestSampleConfiguration() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='left' t='left(1)' />
                        <add name='right' t='right(2)' />
                        <add name='padleft' t='padleft(10,0)' />
                        <add name='padright' t='padright(10).left(10)' />
                    </fields>
                </cfg>
            ";

            var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            Assert.AreEqual(4, sample.Fields.Count());
            Assert.AreEqual("left(1)", sample.Fields[0].T);
            Assert.AreEqual(1, sample.Fields[0].Transforms.Count);

            var left = sample.Fields[0].Transforms.First();
            Assert.AreEqual("left", left.Method);
            Assert.AreEqual(1, left.Length);

            Assert.AreEqual(2, sample.Fields[3].Transforms.Count);

            var first = sample.Fields[3].Transforms.First();
            Assert.AreEqual("padright", first.Method);
            Assert.AreEqual(10, first.TotalWidth);
            Assert.AreEqual('0', first.PaddingChar);

            var last = sample.Fields[3].Transforms.Last();
            Assert.AreEqual("left", last.Method);
            Assert.AreEqual(10, last.Length);
        }
    }

    public class ShTestCfg : CfgNode {
        public ShTestCfg(string cfg, string shorthand) {
            LoadShorthand(shorthand);
            Load(cfg);
        }

        [Cfg(required = true)]
        public List<ShTestField> Fields { get; set; }
    }

    public class ShTestField : CfgNode {

        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg(required = true, shorthand = true)]
        public string T { get; set; }

        [Cfg()]
        public List<ShTestTransform> Transforms { get; set; }
    }

    public class ShTestTransform : CfgNode {
        [Cfg(required = true)]
        public string Method { get; set; }

        [Cfg()]
        public int TotalWidth { get; set; }
        [Cfg()]
        public char PaddingChar { get; set; }
        [Cfg()]
        public int Length { get; set; }
    }
}
