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
    public class FixIssue2 {

        [TestMethod]
        public void Test() {
            const string xml = @"
    <test>
        <things>
            <add value='one' />
            <add value='two' />
        </things>
    </test>
";

            var cfg = new TestIssue2(xml);

            foreach (var error in cfg.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.IsTrue(cfg.Errors()[0] == "A test has an invalid things. If you need a things, decorate it with the [Cfg].");

        }

    }

    public class TestIssue2 : CfgNode {
        // i forgot to put the [Cfg]
        public List<Issue2Thing> Things { get; set; }
        public TestIssue2(string xml) {
            Load(xml);
        }
    }

    public class Issue2Thing : CfgNode {
        [Cfg]
        public string Value { get; set; }
    }

}
