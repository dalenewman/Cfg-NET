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
    public class Domain {

        [Test]
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
            Assert.AreEqual("An invalid value of 'bad-value' is in the 'value' attribute.  The valid domain is: good-value, another-good-value.", problems[0]);

        }

        [Test]
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
            Assert.AreEqual("An invalid value of 'bad-value' is in the 'value' attribute.  The valid domain is: good-value, another-good-value.", problems[0]);

        }

        [Test]
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
            Assert.AreEqual("An invalid value of 'bad' is in the 'value' attribute.  The valid domain is: GOOD, VALUE, ANOTHER, GOOD, VALUE.", problems[0]);

        }

        [Test]
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
            Assert.AreEqual("An invalid value of 'bad-value' is in the 'value' attribute.  The valid domain is: good-value, another-good-value.", problems[0]);

        }
    }

    public class TestCfg : CfgNode {
        [Cfg()]
        public List<TestThing> Things { get; set; }
        public TestCfg(string xml) {
            Load(xml);
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
