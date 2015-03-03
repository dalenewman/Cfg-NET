using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    [TestFixture]
    public class Case {

        [Test]
        public void Test() {
            var xml = @"
    <test>
        <things>
            <add lowerValue='Proper Case!' />
            <add upperValue='Proper Case!' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCase(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(0, problems.Count);

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
