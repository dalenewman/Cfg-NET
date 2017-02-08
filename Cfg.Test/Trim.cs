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
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class Trim {

        [Test]
        public void TrimXml() {
            var xml = @"
    <test>
        <things>
            <add trimmed=' trim me 
' un-trimmed=' i am not trimmed '/>
            <add trimmed='i need to be trimmed ' />
            <add trimmed=' i like the trim' />
        </things>
    </test>
".Replace("'", "\"");

            var cfg = new TestTrim(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("trim me", cfg.Things[0].Trimmed);
            Assert.AreEqual(" i am not trimmed ", cfg.Things[0].Untrimmed);
            Assert.AreEqual("i need to be trimmed", cfg.Things[1].Trimmed);
            Assert.AreEqual("i like the trim", cfg.Things[2].Trimmed);

        }

    }

    public class TestTrim : CfgNode {
        [Cfg()]
        public List<TrimThing> Things { get; set; }
        public TestTrim(string xml) {
            Load(xml);
        }
    }

    public class TrimThing : CfgNode {
        [Cfg(trim=true)]
        public string Trimmed { get; set; }

        [Cfg(trim=false)]
        public string Untrimmed { get; set; }

        [Cfg(trimStart = true)]
        public string StartTrimmed { get; set; }

        [Cfg(trimEnd = true)]
        public string EndTrimmed { get; set; }
    }

}
