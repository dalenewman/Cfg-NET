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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class FreeFormData {

        [Test]
        public void TestXml() {
            var xml = @"
<message>
    <dataSets>
        <add name='ds1'>
            <rows>
                <add undefined1='v1' undefined2='v2' />
                <add undefined1='v3' undefined2='v4' />
            </rows>
        </add>
    </dataSets>
</message>
".Replace("'", "\"");

            var cfg = new TestMessage(xml);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);

            Assert.AreEqual("v1", cfg.DataSets.First().Rows.First()["undefined1"]);
            Assert.AreEqual("v2", cfg.DataSets.First().Rows.First()["undefined2"]);

            Assert.AreEqual("v3", cfg.DataSets.First().Rows.Last()["undefined1"]);
            Assert.AreEqual("v4", cfg.DataSets.First().Rows.Last()["undefined2"]);

        }

        /*
        [Test]
        public void TestJson() {
            var json = @"{
        'things': [
            { 'lowerValue':'Proper Case!'},
            { 'upperValue':'Proper Case!'}
        ]
    }".Replace("'", "\"");

            var cfg = new TestCase(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Count);

            Assert.AreEqual("proper case!", cfg.Things.First().LowerValue);
            Assert.AreEqual("PROPER CASE!", cfg.Things.Last().UpperValue);
        } */

    }

    public class TestMessage : CfgNode {

        [Cfg()]
        public List<CfgDataSet> DataSets { get; set; }

        public TestMessage(string xml) {
            Load(xml);
        }
    }

    public class CfgDataSet : CfgNode {
        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg]
        public List<CfgRow> Rows { get; set; }
    }

    public class CfgRow : IProperties {
        private readonly IDictionary<string, object> _storage;

        public CfgRow() {
            _storage = new Dictionary<string, object>();
        }

        public object this[string name]
        {
            get { return _storage[name]; }
            set { _storage[name] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_storage).GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _storage.GetEnumerator();
        }
    }
}
