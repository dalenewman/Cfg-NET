using System;
using System.Collections.Generic;
using Cfg.Net;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ToJson {

        [Test]
        public void TestSome() {

            const string json = @"{
    ""parameters"":[
        { ""name"":""p1"", ""value"":true },
        { ""name"":""p2"", ""value"":false }
    ]
}";

            const string expected = @"{
    ""parameters"":[
        { ""name"":""p1"", ""value"":true },
        { ""name"":""p2"", ""value"":false }
    ]
}";

            var cfg = new TestToJson(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            var actual = cfg.Serialize();
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual);

            Assert.AreEqual(@"{ ""name"":""p1"", ""value"":true }", cfg.Parameters[0].Serialize());

        }


        class TestToJson : CfgNode {
            [Cfg]
            public List<TestToJsonParameter> Parameters { get; set; }

            public TestToJson(string json) {
                Load(json);
            }
        }

        class TestToJsonParameter : CfgNode {
            [Cfg(required = true, toLower = true)]
            public string Name { get; set; }

            [Cfg(value = false)]
            public bool Value { get; set; }
        }

    }

}
