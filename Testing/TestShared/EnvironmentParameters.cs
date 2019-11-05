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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class EnvironmentParameters {

        [TestMethod]
        public void TestXml() {
            var xml = @"
    <cfg environment='two'>
        <environments>
            <add name='one'>
                <parameters>
                    <add name='p1' value='one-1' />
                    <add name='p2' value='one-2' />
                    <add name='p4' value='0' />
                </parameters>
            </add>
            <add name='two'>
                <parameters>
                    <add name='p1' value='two-1' />
                    <add name='p2' value='two-2' />
                </parameters>
            </add>
        </environments>
        <things>
            <add name='thing-1' value='@(p1)' int-value='1' />
            <add name='thing-2' value='@(p2)' invalid='@(p3)' int-value='@(p4)' />
        </things>
        <free-form-things>
            <add anything='something' />
            <add something='@(p1)' />
        </free-form-things>
    </cfg>
".Replace("'", "\"");

            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"p1", "i am the new p1"}
            };

            var cfg = new MyCfg(xml, parameters);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(1, cfg.Errors().Length);

            Assert.AreEqual("Missing parameter for place-holder @(p3).", cfg.Errors()[0]);

            Assert.AreEqual(2, cfg.Environments.Count);
            Assert.AreEqual(false, cfg.Environment == cfg.Environments[0].Name);
            Assert.AreEqual(true, cfg.Environment == cfg.Environments[1].Name);

            Assert.AreEqual("i am the new p1", cfg.Things[0].Value, "I should be passed in for p1.");
            Assert.AreEqual("two-2", cfg.Things[1].Value, "I am the default value for p2 in the default environment two.");

            Assert.AreEqual("i am the new p1", cfg.FreeFormThings[1]["something"], "Even free form things should be subject to customizers");

        }


        [TestMethod]
        public void TestJson() {
            var json = @"
    {
        'environment':'two',
        'environments': [ {
                'name':'one',
                'parameters':[
                    { 'name':'p1', 'value':'one-1' }
                    { 'name':'p2', 'value':'one-2' }
                ]
            },
            {
                'name':'two',
                'parameters':[
                    { 'name':'p1', 'value':'two-1' }
                    { 'name':'p2', 'value':'two-2' }
                ]
            }
        ],
        'things':[
            { 'name':'thing-1', 'value':'@(p1)' }
            { 'name':'thing-2', 'value':'@(p2)' }
        ],
        'free-form-things':[
            { 'something':'@(p1)' }
        ]
    }
".Replace("'", "\"");

            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"p1", "i am the new p1"}
            };

            var cfg = new MyCfg(json, parameters);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(0, cfg.Errors().Length);

            Assert.AreEqual(2, cfg.Environments.Count);
            Assert.AreEqual(false, cfg.Environment == cfg.Environments[0].Name);
            Assert.AreEqual(true, cfg.Environment == cfg.Environments[1].Name);

            Assert.AreEqual("i am the new p1", cfg.Things[0].Value, "I should be passed in for p1.");
            Assert.AreEqual("two-2", cfg.Things[1].Value, "I am the default value for p2 in the default environment two.");

            Assert.AreEqual("i am the new p1", cfg.FreeFormThings[0]["something"], "Even free form things should be subject to customizers");

        }

    }

    /// <summary>
    /// Should be composed at composition root.
    /// </summary>
    public class MyCfg : CfgNode {
        public MyCfg(string xml, IDictionary<string, string> parameters = null) : base(new EnvironmentModifier()) {
            Load(xml, parameters);
        }

        [Cfg(value = "")]
        public string Environment { get; set; }

        [Cfg(required = false)]
        public List<MyEnvironment> Environments { get; set; }

        [Cfg(required = true)]
        public List<MyThing> Things { get; set; }

        [Cfg]
        public List<MyFreeFormThing> FreeFormThings { get; set; }
    }

    public class MyThing : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public string Value { get; set; }

        [Cfg(value = "hi")]
        public string Invalid { get; set; }

        [Cfg(value=0)]
        public int IntValue { get; set; }
    }

    public class MyFreeFormThing : IProperties
    {
        public object[] Storage;
        public Dictionary<string, short> Map { get; set; }

        public MyFreeFormThing(string[] names) {
            Storage = new object[names.Length];
            Map = new Dictionary<string, short>(names.Length);
            for (short i = 0; i < Convert.ToInt16(names.Length); i++) {
                var name = names[i];
                Map[name] = i;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return Map.ToDictionary(item => item.Key, item => Storage[item.Value]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public object this[string name] {
            get { return Storage[Map[name]]; }
            set { Storage[Map[name]] = value; }
        }
    }

    public class MyEnvironment : CfgNode {

        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public List<MyParameter> Parameters { get; set; }

    }

    public class MyParameter : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        [Cfg(required = true)]
        public string Value { get; set; }
    }
}
