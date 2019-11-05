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
using System.Linq;
using Cfg.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class Case {

        [TestMethod]
        public void TestXml() {
            var xml = @"
    <test>
        <things>
            <add lowerValue='Proper Case!' />
            <add upperValue='Proper Case!' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCase(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("proper case!", cfg.Things.First().LowerValue);
            Assert.AreEqual("PROPER CASE!", cfg.Things.Last().UpperValue);

        }

        [TestMethod]
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
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("proper case!", cfg.Things.First().LowerValue);
            Assert.AreEqual("PROPER CASE!", cfg.Things.Last().UpperValue);
        }

    }

    public class TestCase : CfgNode {
        [Cfg()]
        public List<CaseThing> Things { get; set; }
        public TestCase(string xml) {
            Load(xml);
        }
    }

    public class CaseThing : CfgNode {
        [Cfg(toUpper=true)]
        public string UpperValue { get; set; }

        [Cfg(toLower = true)]
        public string LowerValue { get; set; }
    }

}
