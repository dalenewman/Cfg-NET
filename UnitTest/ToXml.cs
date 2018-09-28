// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2018 Dale Newman
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

using System;
using System.Collections.Generic;
using Cfg.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class ToXml {

        [TestMethod]
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

            const string expected = @"<xml>
    <parameters>
        <add name=""p1"" value=""true"" />
        <add name=""p2"">
            <alternatives>
                <add idea=""an idea"" />
            </alternatives>
        </add>
    </parameters>
</xml>";

            var cfg = new TestToXml(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            var actual = cfg.Serialize();
            Console.WriteLine(actual);
            Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));

            Assert.AreEqual("<add name=\"p1\" value=\"true\" />", cfg.Parameters[0].Serialize());

        }

        [Cfg(name="xml")]
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
