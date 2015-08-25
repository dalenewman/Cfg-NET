using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Parsers;
using Cfg.Test.Parsers;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class DifferentXmlParser {

        [Test]
        public void TestXml() {
            var xml = @"
<xml>
    <parameters>
        <add name='p1' value='true' />
        <add name='p2' value='false' />
    </parameters>
</xml>".Replace("'", "\"");

            var cfg = new TestDifferentXmlParser(xml, new XDocumentParser());

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

        }

    }

    public sealed class TestDifferentXmlParser : CfgNode {
        [Cfg]
        public List<TestDifferentXmlParserParameter> Parameters { get; set; }

        public TestDifferentXmlParser(string xml, IParser parser)
            : base(parser: parser) {
            Load(xml);
        }
    }

    public class TestDifferentXmlParserParameter : CfgNode {
        [Cfg(required = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(value = false)]
        public bool Value { get; private set; }
    }
}
