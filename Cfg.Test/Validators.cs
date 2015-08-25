using System;
using System.Collections.Generic;
using Cfg.Net;
using Cfg.Net.Contracts;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class Validators {

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
                "The value 'bad-value' in the 'value' attribute, inside 'things' is no good! It does not have two dashes like we agreed on.",
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
            Assert.AreEqual(
                "The value 'bad-value' in the 'value' attribute, inside 'things' is no good! It does not have two dashes like we agreed on.",
                problems[0]);

        }

        [Test]
        public void TestMultipleValidators() {
            var xml = @"
    <test>
        <things>
            <add value='this-GOOD-value' />
            <add value='this-BAD-value' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestValidatorCfg2(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("The value 'this-bad-value' is missing good! I am deeply offended.", problems[0]);

        }

        public class TestValidatorCfg : CfgNode {
            [Cfg]
            public List<TestValidatorThing> Things { get; set; }

            public TestValidatorCfg(string xml)
                : base(validators: new Dictionary<string, IValidator> { { "2dashes", new Contains2Dashes() } }) {
                Load(xml);
            }
        }

        public class TestValidatorThing : CfgNode {
            [Cfg(validators = "2dashes", toLower = true)]
            public string Value { get; set; }
        }

        public class TestValidatorCfg2 : CfgNode {
            [Cfg]
            public List<TestValidatorThing2> Things { get; set; }

            public TestValidatorCfg2(string xml)
                : base(validators: new Dictionary<string, IValidator>
                {
                    {"2dashes", new Contains2Dashes()},
                    {"contains,good", new ContainsGood()}
                }) {
                Load(xml);
            }
        }

        public class TestValidatorThing2 : CfgNode {
            [Cfg(validators = "2dashes|contains,good", validatorDelimiter = '|', toLower = true)]
            public string Value { get; set; }
        }

        public class Contains2Dashes : IValidator {

            public ValidatorResult Validate(string parent, string name, object value) {
                var result = new ValidatorResult();
                var count = value.ToString().Split(new[] { '-' }, StringSplitOptions.None).Length - 1;
                result.Valid = count == 2;
                if (!result.Valid) {
                    result.Error(
                        "The value '{0}' in the '{1}' attribute, inside '{2}' is no good! It does not have two dashes like we agreed on.",
                        value, name, parent);
                }
                return result;
            }

        }

        public class ContainsGood : IValidator {

            public ValidatorResult Validate(string parent, string name, object value) {
                var result = new ValidatorResult { Valid = value.ToString().Contains("good") };
                if (!result.Valid) {
                    result.Error("The value '{0}' is missing good! I am deeply offended.", value);
                }
                return result;
            }

        }

    }
}
