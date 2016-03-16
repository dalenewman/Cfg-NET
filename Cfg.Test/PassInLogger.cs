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
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Loggers;
using NUnit.Framework;

namespace Cfg.Test {
    [TestFixture]
    public class PassInLogger {

        [Test]
        public void TestXml() {
            var xml = @"
<xml>
    <parameters>
        <add name='p1' value='true' />
        <add name='p2' value='falses' />
    </parameters>
</xml>".Replace("'", "\"");

            var cfg = new TestPassInLogger(xml, new TraceLogger());

            Assert.AreEqual(1, cfg.Errors().Length);

            Assert.AreEqual(true, cfg.Parameters.First().Value);
            Assert.AreEqual(false, cfg.Parameters.Last().Value);

        }

    }

    public sealed class TestPassInLogger : CfgNode {
        [Cfg()]
        public List<TestPassInLoggerParameter> Parameters { get; set; }

        public TestPassInLogger(string xml, ILogger externalLogger)
            : base(externalLogger) {
            Load(xml);
        }
    }

    public class TestPassInLoggerParameter : CfgNode {
        [Cfg(required = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(value = false)]
        public bool Value { get; set; }
    }
}
