using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Loggers;

namespace Cfg.Test {

    [TestFixture]
    public class Case {

        [Test]
        public void TestXml() {
            var xml = @"
    <test>
        <things>
            <add lowerValue='Proper Case!' />
            <add upperValue='Proper Case!' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCase(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("proper case!", cfg.Things.First().LowerValue);
            Assert.AreEqual("PROPER CASE!", cfg.Things.Last().UpperValue);

        }

        [Test]
        public void TestJson() {
            var json = @"{
        'things': [
            { 'lowerValue':'Proper Case!'},
            { 'upperValue':'Proper Case!'}
        ]
    }".Replace("'", "\"");

            var cfg = new TestCase(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("proper case!", cfg.Things.First().LowerValue);
            Assert.AreEqual("PROPER CASE!", cfg.Things.Last().UpperValue);
        }

    }

    public class TestCase : CfgNode {
        [Cfg()]
        public List<CaseThing> Things { get; set; }
        public TestCase(string xml) {
            Load(xml);
        }
    }

    public class CaseThing : CfgNode {
        [Cfg(toUpper=true)]
        public string UpperValue { get; set; }

        [Cfg(toLower = true)]
        public string LowerValue { get; set; }
    }

}
