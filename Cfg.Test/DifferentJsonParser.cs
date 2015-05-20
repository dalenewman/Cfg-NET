using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cfg.Test.Parsers;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Parsers;
using Transformalize.Libs.Cfg.Net.Loggers;

namespace Cfg.Test {

    [TestFixture]
    public class DifferentJsonParser {

        [Test]
        public void TestJson() {
            var json = @"
{
    'parameters' : [
        { 'name':'p1', 'value':true },
        { 'name':'p2', 'value':false }
    ]
}".Replace("'", "\"");

            var cfg = new TestDifferentJsonParser(json, new JsonNetParser());

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

        }

    }

    public sealed class TestDifferentJsonParser : CfgNode {
        [Cfg()]
        public List<TestDifferentJsonParserParameter> Parameters { get; set; }

        public TestDifferentJsonParser(string xml, IParser parser):base(parser) {
            Load(xml);
        }
    }

    public class TestDifferentJsonParserParameter : CfgNode {
        [Cfg(required = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(value = false)]
        public bool Value { get; set; }
    }
}
