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
using Cfg.Net.Parsers.YamlDotNet;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cfg.Test {

    [TestFixture]
    public class YamlDotNet {

        [Test]
        public void TestYaml() {
            const string yaml = @"parameters:
    - name: p1
      value: true
    - name: p2
      value: false
";

            var cfg = new TestYamlParser(yaml, new YamlDotNetParser(), new YamlDotNetSerializer(SerializationOptions.EmitDefaults, new HyphenatedNamingConvention()));

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

            var backToYaml = cfg.Serialize();
            Assert.AreEqual(@"parameters:
- name: p1
  value: true
  sequence: 1
- name: p2
  value: false
  sequence: 2
sequence: 0
".Replace("\r\n", "\n"), backToYaml.Replace("\r\n", "\n"));

        }


        class TestYamlParser : CfgNode {
            [Cfg]
            public List<TestYamlParserParameter> Parameters { get; set; }

            public TestYamlParser(string yaml, params IDependency[] dependencies) : base(dependencies) {
                Load(yaml);
            }
        }

        class TestYamlParserParameter : CfgNode {
            [Cfg(required = true, toLower = true)]
            public string Name { get; set; }

            [Cfg(value = false)]
            public bool Value { get; set; }
        }

    }

}
