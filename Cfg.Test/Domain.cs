using System;
using System.Collections.Generic;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    [TestFixture]
    public class Domain {

        [Test]
        public void Test() {
            var xml = @"
    <test>
        <things>
            <add value='good-value' />
            <add value='bad-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCfg(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("A 'things' 'add' element has an invalid value of 'bad-value' in the 'value' attribute.  The valid domain is: good-value, another-good-value.", problems[0]);

        }

        [Test]
        public void TestDifferentDomainDelimiter() {
            var xml = @"
    <test>
        <things>
            <add value='good' />
            <add value='bad' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCfg2(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("A 'things' 'add' element has an invalid value of 'bad' in the 'value' attribute.  The valid domain is: GOOD, VALUE, ANOTHER, GOOD, VALUE.", problems[0]);

        }

        [Test]
        public void TestDomainAndToLower() {
            var xml = @"
    <test>
        <things>
            <add value='GOOD-value' />
            <add value='bad-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCfg(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("A 'things' 'add' element has an invalid value of 'bad-value' in the 'value' attribute.  The valid domain is: good-value, another-good-value.", problems[0]);

        }
    }

    public class TestCfg : CfgNode {
        [Cfg()]
        public List<TestThing> Things { get; set; }
        public TestCfg(string xml) {
            Load(xml);
        }
    }

    public class TestThing : CfgNode {
        [Cfg(domain = "good-value,another-good-value", ignoreCase = true)]
        public string Value { get; set; }
    }

    public class TestCfg2 : CfgNode {
        [Cfg()]
        public List<TestThing2> Things { get; set; }
        public TestCfg2(string xml) {
            Load(xml);
        }
    }

    public class TestThing2 : CfgNode {
        [Cfg(domain = "GOOD-VALUE-ANOTHER-GOOD-VALUE", domainDelimiter = '-', ignoreCase = true)]
        public string Value { get; set; }
    }

}
