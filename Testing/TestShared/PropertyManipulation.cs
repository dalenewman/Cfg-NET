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
using Cfg.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class PropertyManipulation {

        [TestMethod]
        public void TestXml() {
            var xml = @"
    <cfg thing1='System.Int16' thing2='System.Int16'>
    </cfg>
".Replace("'", "\"");

            var cfg = new Pm(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
            Assert.AreEqual("An invalid value of System.Int16 is in thing1.  The valid domain is: int16.", problems[0]);
            Assert.AreEqual("System.Int16", cfg.Thing1);
            Assert.AreEqual("int16", cfg.Thing2);

        }

        public void TestJson() {
            var json = @"{ 'thing1':'System.Int16', 'thing2':'System.Int16' }".Replace("'", "\"");

            var cfg = new Pm(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(1, problems.Length);
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
