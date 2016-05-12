#region license
// Cfg.Net
// Copyright 2015 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
        { ""namers"":""p1"", ""value"":true },
        { ""namers"":""p2"" }
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

            Assert.AreEqual(@"{ ""namers"":""p1"", ""value"":true }", cfg.Parameters[0].Serialize());

        }


        class TestToJson : CfgNode {
            [Cfg]
            public List<TestToJsonParameter> Parameters { get; set; }

            public TestToJson(string json) {
                Load(json);
            }
        }

        class TestToJsonParameter : CfgNode {
            [Cfg(required = true, toLower = true,name="namers")]
            public string Name { get; set; }

            [Cfg(value = false)]
            public bool Value { get; set; }

            [Cfg(value = true, serialize = false)]
            public bool Hidden { get; set; }
        }

    }

}
