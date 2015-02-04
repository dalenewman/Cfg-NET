using System;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    [TestFixture]
    public class PropertyManipulation {

        [Test]
        public void Test() {
            var xml = @"
    <cfg thing1='System.Int16' thing2='System.Int16'>
    </cfg>
".Replace("'", "\"");

            var cfg = new Pm(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Problems();
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("The root element has an invalid value of 'System.Int16' in the 'thing1' attribute.  The valid domain is: int16.", problems[0]);
            Assert.AreEqual("System.Int16", cfg.Thing1);
            Assert.AreEqual("int16", cfg.Thing2);

        }
    }

    public class Pm : CfgNode {
        private string _thing2;

        [Cfg(domain = "int16")]
        public string Thing1 { get; set; }

        [Cfg(domain = "int16")]
        public string Thing2 {
            get { return _thing2; }
            set {
                _thing2 = value != null && value.StartsWith("Sy", StringComparison.OrdinalIgnoreCase) ? value.ToLower().Replace("system.", string.Empty) : value;
            }
        }

        public Pm(string xml) {
            Load(xml);
        }
    }

}
