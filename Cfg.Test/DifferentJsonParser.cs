#region license
// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Parsers.Json.Net;
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

            var cfg = new TestDifferentJsonParser(json, new JsonNetParser(), new JsonNetSerializer());

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

            var backToJson = cfg.Serialize();
            Assert.AreEqual("{\"parameters\":[{\"name\":\"p1\",\"value\":true,\"sequence\":1},{\"name\":\"p2\",\"value\":false,\"sequence\":2}],\"sequence\":0}", backToJson);

        }


        class TestDifferentJsonParser : CfgNode {
            [Cfg]
            public List<TestDifferentJsonParserParameter> Parameters { get; set; }

            public TestDifferentJsonParser(string xml, params IDependency[] dependencies) : base(dependencies) {
                Load(xml);
            }
        }

        class TestDifferentJsonParserParameter : CfgNode {
            [Cfg(required = true, toLower = true)]
            public string Name { get; set; }

            [Cfg(value = false)]
            public bool Value { get; set; }
        }

    }

}
