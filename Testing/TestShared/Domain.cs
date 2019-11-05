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
    public class Domain {

        [TestMethod]
        public void TestXml() {
            var xml = @"
    <test>
        <things>
            <add value='good-value' />
            <add value='bad-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCfg(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("An invalid value of bad-value is in value.  The valid domain is: good-value, another-good-value.", problems[0]);

        }

        [TestMethod]
        public void TestJson() {
            var json = @"{
        'things': [
            { 'value':'good-value' },
            { 'value':'bad-value' }
        ]
    }".Replace("'", "\"");

            var cfg = new TestCfg(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("An invalid value of bad-value is in value.  The valid domain is: good-value, another-good-value.", problems[0]);

        }

        [TestMethod]
        public void TestDifferentDomainDelimiter() {
            var xml = @"
    <test>
        <things>
            <add value='good' />
            <add value='bad' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCfg2(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("An invalid value of bad is in value.  The valid domain is: GOOD, VALUE, ANOTHER, GOOD, VALUE.", problems[0]);

        }

        [TestMethod]
        public void TestDomainAndToLower() {
            var xml = @"
    <test>
        <things>
            <add value='GOOD-value' />
            <add value='bad-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestCfg(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("An invalid value of bad-value is in value.  The valid domain is: good-value, another-good-value.", problems[0]);

        }
    }

    public class TestCfg : CfgNode {
        [Cfg()]
        public List<TestThing> Things { get; set; }
        public TestCfg(string xml, IDictionary<string, string> parameters = null) {
            Load(xml, parameters ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }
    }

    public class TestThing : CfgNode {
        [Cfg(domain = "good-value,another-good-value", ignoreCase = true)]
        public string Value { get; set; }
    }

    public class TestCfg2 : CfgNode {
        [Cfg()]
        public List<TestThing2> Things { get; set; }
        public TestCfg2(string xml) {
            Load(xml);
        }
    }

    public class TestThing2 : CfgNode {
        [Cfg(domain = "GOOD-VALUE-ANOTHER-GOOD-VALUE", delimiter = '-', ignoreCase = true)]
        public string Value { get; set; }
    }

}
