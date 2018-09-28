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
using System.Linq;
using Cfg.Net;
using Cfg.Net.Ext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {
    [TestClass]
    public class Clone {

        [TestMethod]
        public void Test1() {
            const string xml = @"<clone setting='cloner'>
    <items>
        <add name='item1' number='1'>
            <subItems>
                <add on='true' />
                <add on='false' />
            </subItems>
        </add>
        <add name='item2' number='2'>
            <subItems>
                <add on='false' />
                <add on='true' />
            </subItems>
        </add>
    </items>
</clone>";

            var original = new CloneRoot(xml);

           foreach (var error in original.Errors()) {
                Console.Error.WriteLine(error);
            }

            Assert.AreEqual(0, original.Errors().Length);
            Assert.AreEqual("cloner", original.Setting);
            Assert.AreEqual(2, original.Items.Count);
            Assert.AreEqual("item2", original.Items.Last().Name);
            Assert.AreEqual(true, original.Items.Last().SubItems.Last().On);


            var clone = original.Clone() as CloneRoot;

            Assert.AreEqual(0, clone.Errors().Length);
            Assert.AreEqual("cloner", clone.Setting);
            Assert.AreEqual(2, clone.Items.Count);
            Assert.AreEqual("item2", clone.Items.Last().Name);
            Assert.AreEqual(true, clone.Items.Last().SubItems.Last().On);

            clone.Items.Last().SubItems.Clear();

            Assert.AreEqual(0, clone.Items.Last().SubItems.Count);
            Assert.AreEqual(2, original.Items.Last().SubItems.Count);

            clone.Items.First().Number = 7;

            Assert.AreEqual(7, clone.Items.First().Number);
            Assert.AreEqual(1, original.Items.First().Number);

        }


        class CloneRoot : CfgNode {
            public CloneRoot() { }

            public CloneRoot(string xml) {
                Load(xml);
            }

            [Cfg(value = "Yes")]
            public string Setting { get; set; }

            [Cfg()]
            public List<CloneItem> Items { get; set; }
        }

        class CloneItem : CfgNode {
            [Cfg(unique = true, required = true)]
            public string Name { get; set; }

            [Cfg(value = 7)]
            public int Number { get; set; }

            [Cfg()]
            public List<CloneSubItem> SubItems { get; set; }
        }

        class CloneSubItem : CfgNode {
            [Cfg(value = true)]
            public bool On { get; set; }
        }

    }
}
