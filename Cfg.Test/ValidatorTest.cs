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
using Cfg.Net;
using Cfg.Net.Contracts;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ValidatorTest {

        [Test]
        public void TestXml() {
            var xml = @"
    <test>
        <things>
            <add value='this-GOOD-value' />
            <add value='BAD-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestValidatorCfg(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual(
                "The value 'BAD-value' in the 'value' attribute is no good! It does not have two dashes like we agreed on.",
                problems[0]);

        }

        [Test]
        public void TestJson() {
            var json = @"{
        'things': [
            { 'value':'this-GOOD-value' },
            { 'value':'BAD-value' }
        ]
    }".Replace("'", "\"");

            var cfg = new TestValidatorCfg(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("The value 'BAD-value' in the 'value' attribute is no good! It does not have two dashes like we agreed on.", problems[0]);

        }

        [Test]
        public void TestMultipleValidators() {
            var xml = @"
    <test>
        <things>
            <add value='this-good-value' />
            <add value='this-BAD-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestValidatorCfg2(xml, new Contains2Dashes(), new ContainsGood());
            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("The value 'this-BAD-value' is missing good! I am deeply offended.", problems[0]);

        }

        public class TestValidatorCfg : CfgNode {
            [Cfg]
            public List<TestValidatorThing> Things { get; set; }

            public TestValidatorCfg(string xml)
                : base(new Contains2Dashes()) {
                Load(xml);
            }
        }

        public class TestValidatorThing : CfgNode {
            [Cfg(toLower = true)]
            public string Value { get; set; }
        }

        public class TestValidatorCfg2 : CfgNode {

            [Cfg]
            public List<TestValidatorThing2> Things { get; set; }

            public TestValidatorCfg2(string xml, params IDependency[] validators) : base(validators) {
                Load(xml);
            }
        }

        public class TestValidatorThing2 : CfgNode {
            [Cfg(toLower = true)]
            public string Value { get; set; }
        }

        public class Contains2Dashes : ICustomizer {
            public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger) {

                if (parent != "things")
                    return;

                IAttribute attr;
                if (!node.TryAttribute("value", out attr))
                    return;
                if (attr.Value == null) return;

                var strValue = attr.Value.ToString();

                var count = strValue.Split(new[] { '-' }, StringSplitOptions.None).Length - 1;
                if (count != 2) {
                    logger.Error("The value '{0}' in the '{1}' attribute is no good! It does not have two dashes like we agreed on.", strValue, attr.Name);
                }

            }

            public void Customize(INode node, IDictionary<string, string> parameters, ILogger logger) { }
        }

        public class ContainsGood : ICustomizer {
            public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger) {
                if (parent != "things")
                    return;

                IAttribute attr;
                if (!node.TryAttribute("value", out attr))
                    return;
                if (attr.Value == null) return;

                var strValue = attr.Value.ToString();

                if (!strValue.Contains("good")) {
                    logger.Error("The value '{0}' is missing good! I am deeply offended.", strValue);
                }
            }

            void ICustomizer.Customize(INode root, IDictionary<string, string> parameters, ILogger logger) { }
        }

    }
}
