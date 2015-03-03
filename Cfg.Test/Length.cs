using System;
using System.Collections.Generic;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    [TestFixture]
    public class Length {

        [Test]
        public void Test() {
            var xml = @"
    <test>
        <things>
            <add value='too-short' />
            <add value='this-is-way-too-long' />
            <add value='just-right' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestLength(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(2, problems.Count);
            Assert.IsTrue(problems.Contains("The `value` attribute's value `too-short` is too short. It's 9 characters. It must be at least 10 characters."));
            Assert.IsTrue(problems.Contains("The `value` attribute's value `this-is-way-too-long` is too long. It's 20 characters. It must not exceed 15 characters."));

        }

    }

    public class TestLength : CfgNode {
        [Cfg()]
        public List<LengthThing> Things { get; set; }
        public TestLength(string xml) {
            Load(xml);
        }
    }

    public class LengthThing : CfgNode {
        [Cfg(minLength = 10, maxLength = 15)]
        public string Value { get; set; }
    }

}
