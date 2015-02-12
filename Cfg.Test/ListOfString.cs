using System;
using System.Collections.Generic;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    [TestFixture]
    public class ListOfString {

        [Test]
        public void Test() {
            var xml = @"
    <cfg>
        <strings>
            <add value='1' />
            <add value='2' />
            <add value='3' />
        </strings>
    </cfg>
".Replace("'", "\"");

            var cfg = new Los(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(3, cfg.Strings.Count);

        }
    }

    [TestFixture]
    public class RequiredListOfNumbers {

        [Test]
        public void Test() {
            var xml = @"
    <cfg>
        <numbers>
            <add>
                <inner-numbers>
                    <add x='1' />
                    <add y='5' />
                    <add z='10' />
                </inner-numbers>
            </add>
        </numbers>
    </cfg>
".Replace("'", "\"");

            var cfg = new Los(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(0, cfg.Strings.Count);
            Assert.AreEqual(1, cfg.Numbers.Count);
            Assert.AreEqual(3, cfg.Numbers[0].InnerNumbers.Count);
            Assert.AreEqual(1, cfg.Numbers[0].InnerNumbers[0]);
            Assert.AreEqual(5, cfg.Numbers[0].InnerNumbers[1]);
            Assert.AreEqual(10, cfg.Numbers[0].InnerNumbers[2]);

        }
    }

    public class Los : CfgNode {
        [Cfg()]
        public List<string> Strings { get; set; }

        [Cfg()]
        public List<LosNumber> Numbers { get; set; }

        public Los(string xml) {
            Load(xml);
        }
    }

    public class LosNumber : CfgNode {
        [Cfg(required = true)]
        public List<int> InnerNumbers { get; set; }
    }
}
