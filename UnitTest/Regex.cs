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
    public class Regex {

        [TestMethod]
        public void Test1() {
            var xml = @"
    <test name='bad-value' invalid='x'>
        <things>
            <add value='goodVALUE' />
            <add value='bad value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestRegex(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(3, problems.Length);
            Assert.AreEqual("invalid has an invalid regex of [ parsing \"[\" - Unterminated [] set.", problems[0]);
            Assert.AreEqual("bad value does not match regex ^[a-z0-9]*$ in value", problems[1]);
            Assert.AreEqual("bad-value does not match regex ^[a-zA-Z]*$ in name", problems[2]);
        }
    }

    public class TestRegex : CfgNode {
        [Cfg]
        public List<TestRegexThing> Things { get; set; }

        [Cfg(regex = @"^[a-zA-Z]*$", value = "")]
        public string Name { get; set; }

        [Cfg(regex = "[")]
        public string Invalid { get; set; }

        public TestRegex(string xml) {
            Load(xml);
        }
    }

    public class TestRegexThing : CfgNode {
        [Cfg(regex = "^[a-z0-9]*$", ignoreCase = true, value="")]
        public string Value { get; set; }
    }


}
