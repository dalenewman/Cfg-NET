﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Test.Parsers;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Parsers;

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

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(0, problems.Count);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

        }

    }

    public sealed class TestDifferentXmlParser : CfgNode {
        [Cfg()]
        public List<TestDifferentXmlParserParameter> Parameters { get; set; }

        public TestDifferentXmlParser(string xml, IParser parser):base(parser) {
            Load(xml);
        }
    }

    public class TestDifferentXmlParserParameter : CfgNode {
        [Cfg(required = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(value = false)]
        public bool Value { get; set; }
    }
}