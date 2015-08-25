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

        public TestDifferentJsonParser(string xml, IParser parser):base(parser:parser) {
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
