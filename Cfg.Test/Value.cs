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
    public class Value {

        [Test]
        public void TestXml() {
            var xml = @"
    <test>
        <things>
            <add value='5' longValue='5' stringValue='1' />
            <add value='10' longValue='10' stringValue='2'/>
            <add value='15' longValue='15' stringValue='6'/>
            <add value='10' longValue='10' stringValue='&quot;10&quot;' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestValue(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(4, problems.Length);
            Assert.IsTrue(problems[0] == "The 'value' attribute value '5' is too small. The minimum value allowed is '6'.");
            Assert.IsTrue(problems[1] == "The 'longvalue' attribute value '5' is too small. The minimum value allowed is '6'.");
            Assert.IsTrue(problems[2] == "The 'stringvalue' attribute value '6' is too big. The maximum value allowed is '5'.");
            Assert.IsTrue(problems[3] == "The 'stringvalue' attribute value '\"10\"' is too small. The minimum value allowed is '1'.");

        }

        [Test]
        public void TestJson() {
            var json = @"{
        'things':[
            { 'value':5, 'longValue':5, 'stringValue':'1' },
            { 'value':10, 'longValue':10, 'stringValue':'2' },
            { 'value':15, 'longValue':15, 'stringValue':'6' }
        ]
    }
".Replace("'", "\"");

            var cfg = new TestValue(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(3, problems.Length);
            Assert.IsTrue(problems[0] == "The 'value' attribute value '5' is too small. The minimum value allowed is '6'.");
            Assert.IsTrue(problems[1] == "The 'longvalue' attribute value '5' is too small. The minimum value allowed is '6'.");
            Assert.IsTrue(problems[2] == "The 'stringvalue' attribute value '6' is too big. The maximum value allowed is '5'.");

        }

    }

    public class TestValue : CfgNode {
        [Cfg()]
        public List<ValueThing> Things { get; set; }

        public TestValue(string xml) {
            Load(xml);
        }
    }

    public class ValueThing : CfgNode {

        [Cfg(minValue = 6, maxValue = 15)]
        public int Value { get; private set; }

        [Cfg(minValue = 6, maxValue = 15)]
        public long LongValue { get; set; }

        [Cfg(minValue = 1, maxValue = 5)]
        public string StringValue { get; set; }
    }

}
