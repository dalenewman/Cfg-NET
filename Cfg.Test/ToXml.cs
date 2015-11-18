using System;
using System.Collections.Generic;
using Cfg.Net;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ToXml {

        [Test]
        public void TestSome() {

            const string xml = @"<xml>
    <parameters>
        <add name='p1' value='true' />
        <add name='p2' value='false'>
            <alternatives>
                <add idea='an idea' />
            </alternatives>
        </add>
    </parameters>
</xml>";

            const string expected = @"<TestToXml>
    <parameters>
        <add name=""p1"" value=""true"" />
        <add name=""p2"">
            <alternatives>
                <add idea=""an idea"" />
            </alternatives>
        </add>
    </parameters>
</TestToXml>";

            var cfg = new TestToXml(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            var actual = cfg.Serialize();
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual);

            Assert.AreEqual("<add name=\"p1\" value=\"true\" />", cfg.Parameters[0].Serialize());

        }

        class TestToXml : CfgNode {
            [Cfg]
            public List<TestToXmlParameter> Parameters { get; set; }

            public TestToXml(string xml) {
                Load(xml);
            }
        }

        class TestToXmlParameter : CfgNode {
            [Cfg(required = true, toLower = true)]
            public string Name { get; set; }

            [Cfg(value = false)]
            public bool Value { get; set; }

            [Cfg()]
            public List<TestToXmlAlt> Alternatives { get; set; }
        }

        class TestToXmlAlt : CfgNode {
            [Cfg(value="")]
            public string Idea { get; set; }
        }

    }

}
