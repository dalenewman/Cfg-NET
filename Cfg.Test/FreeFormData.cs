using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Loggers;

namespace Cfg.Test {

    [TestFixture]
    public class FreeFormData {

        [Test]
        public void TestXml() {
            var xml = @"
<message>
    <dataSets>
        <add name='ds1'>
            <rows>
                <add undefined1='v1' undefined2='v2' />
                <add undefined1='v3' undefined2='v4' />
            </rows>
        </add>
    </dataSets>
</message>
".Replace("'", "\"");

            var cfg = new TestMessage(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("v1", cfg.DataSets.First().Rows.First()["undefined1"]);
            Assert.AreEqual("v2", cfg.DataSets.First().Rows.First()["undefined2"]);

            Assert.AreEqual("v3", cfg.DataSets.First().Rows.Last()["undefined1"]);
            Assert.AreEqual("v4", cfg.DataSets.First().Rows.Last()["undefined2"]);

        }

        /*
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
            Assert.AreEqual(0, problems.Count);

            Assert.AreEqual("proper case!", cfg.Things.First().LowerValue);
            Assert.AreEqual("PROPER CASE!", cfg.Things.Last().UpperValue);
        } */

    }

    public class TestMessage : CfgNode {

        [Cfg()]
        public List<CfgDataSet> DataSets { get; set; }

        public TestMessage(string xml) {
            Load(xml);
        }
    }

    public class CfgDataSet : CfgNode
    {
        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg()]
        public List<Dictionary<string,string>> Rows { get; set; }
    }
}
