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
using Cfg.Net.Contracts;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ModifierTest {

        [Test]
        public void TestIt() {
            const string xml = @"
    <test>
        <things>
            <add value='this--GOOD-value' />
            <add value='BAD--value' />
        </things>
    </test>";

            var cfg = new TestModifierCfg(xml);
            Assert.AreEqual(0, cfg.Errors().Length);
            Assert.AreEqual("this-bad-value", cfg.Things[0].Value, "The GOOD is replaced with BAD, the -- replaced with -, and the whole thing lower case.");
            Assert.AreEqual("bad-value", cfg.Things[1].Value);
        }

        public class TestModifierCfg : CfgNode {
            [Cfg]
            public List<TestModifierThing> Things { get; set; }

            public TestModifierCfg(string xml) : base(new Replace2Dashes(), new ReplaceGoodWithBad()) {
                Load(xml);
            }
        }

        public class TestModifierThing : CfgNode {
            [Cfg(toLower = true)]
            public string Value { get; set; }
        }

        public class Replace2Dashes : ICustomizer {

            public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger) {
                if (parent != "things")
                    return;

                IAttribute attr;
                if (!node.TryAttribute("value", out attr))
                    return;

                var str = attr.Value.ToString();
                attr.Value = str.Replace("--", "-");
            }

            public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger) { }
        }

        public class ReplaceGoodWithBad : ICustomizer {

            public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger) {
                if (parent != "things")
                    return;

                IAttribute attr;
                if (node.TryAttribute("value", out attr)) {
                    attr.Value = attr.Value.ToString().Replace("GOOD", "BAD");
                }

            }

            public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger){}
        }

    }
}
