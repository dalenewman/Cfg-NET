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
    public class EmptyLists {

        [Test]
        public void ShouldBeInitialized() {
            var cfg = new EmptyListCfg();
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(0, cfg.Connections.Count);
            Assert.IsNullOrEmpty(cfg.NullProperty);
            Assert.IsNotNull(cfg.DefaultedProperty);
            Assert.AreEqual("Default", cfg.DefaultedProperty);
        }

        [Test]
        public void ShouldRespectObjectInitialization() {
            var cfg = new EmptyListCfg {
                DefaultedProperty = "Set",
                NullProperty = "Null",
                Connections = new List<EmptyListConnection> { new EmptyListConnection() }
            };

            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(1, cfg.Connections.Count);
            Assert.IsNotNull(cfg.NullProperty);
            Assert.IsNotNull(cfg.DefaultedProperty);
            Assert.AreEqual("Null", cfg.NullProperty);
            Assert.AreEqual("Set", cfg.DefaultedProperty);
        }

        [Test]
        public void TestConnectionsXml() {
            var xml = @"
                <cfg>
                </cfg>
            ".Replace("'", "\"");

            var cfg = new EmptyListCfg(xml);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(0, cfg.Connections.Count);

        }

        [Test]
        public void TestConnectionsJson() {
            var json = "{}";

            var cfg = new EmptyListCfg(json);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(0, cfg.Connections.Count);

        }

        [Test]
        public void TestOtherThingsXml() {
            var xml = @"
                <cfg>
                    <connections>
                        <add provider='file' file='c:\temp.txt' />
                    </connections>
                </cfg>
            ".Replace("'", "\"");

            var cfg = new EmptyListCfg(xml);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(1, cfg.Connections.Count);
            Assert.IsNotNull(cfg.Connections[0].OtherThings);
            Assert.AreEqual(0, cfg.Connections[0].OtherThings.Count);

        }

        [Test]
        public void TestOtherThingsJson() {
            var json = @"{
                    'connections':[
                        { 'provider':'file', 'file':'c:\\temp.txt' }
                    ]
                }
            ".Replace("'", "\"");

            var cfg = new EmptyListCfg(json);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(1, cfg.Connections.Count);
            Assert.IsNotNull(cfg.Connections[0].OtherThings);
            Assert.AreEqual(0, cfg.Connections[0].OtherThings.Count);

        }

    }

    public sealed class EmptyListCfg : CfgNode {

        public EmptyListCfg() {
        }

        public EmptyListCfg(string xml) {
            Load(xml);
        }

        [Cfg()]
        public string NullProperty { get; set; }

        [Cfg(value = "Default")]
        public string DefaultedProperty { get; set; }

        [Cfg(required = false)]
        public List<EmptyListConnection> Connections { get; set; }

    }

    public class EmptyListConnection : CfgNode {

        [Cfg(required = true, domain = "file,folder,other")]
        public string Provider { get; set; }

        [Cfg()]
        public string File { get; set; }

        [Cfg()]
        public string Folder { get; set; }

        [Cfg()]
        public List<EmptyListOtherThing> OtherThings { get; set; }

    }

    public class EmptyListOtherThing : CfgNode {
        [Cfg()]
        public string Name { get; set; }
    }
}
