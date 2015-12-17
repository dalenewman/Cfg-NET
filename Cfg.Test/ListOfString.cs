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
using System;
using System.Collections.Generic;
using Cfg.Net;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ListOfString {

        [Test]
        public void TestXml() {
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

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(3, cfg.Strings.Count);

        }

        [Test]
        public void TestJson() {
            var json = @"
    {
        'strings': [
            { 'value':'1' },
            { 'value':'2' },
            { 'value':'3' }
        ]
    }
".Replace("'", "\"");

            var cfg = new Los(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(3, cfg.Strings.Count);

        }

    }

    [TestFixture]
    public class RequiredListOfNumbers {

        [Test]
        public void TestXml() {
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

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(0, cfg.Strings.Count);
            Assert.AreEqual(1, cfg.Numbers.Count);
            Assert.AreEqual(3, cfg.Numbers[0].InnerNumbers.Count);
            Assert.AreEqual(1, cfg.Numbers[0].InnerNumbers[0]);
            Assert.AreEqual(5, cfg.Numbers[0].InnerNumbers[1]);
            Assert.AreEqual(10, cfg.Numbers[0].InnerNumbers[2]);

        }

        [Test]
        public void TestJson() {
            var json = @"
    {
        'numbers':[
            { 'inner-numbers': [
                    { 'x':1 },
                    { 'y':5 },
                    { 'z':10 }
                ]
            }
        ]
    }
".Replace("'", "\"");

            var cfg = new Los(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(0, cfg.Strings.Count);
            Assert.AreEqual(1, cfg.Numbers.Count);
            Assert.AreEqual(3, cfg.Numbers[0].InnerNumbers.Count);
            Assert.AreEqual(1, cfg.Numbers[0].InnerNumbers[0]);
            Assert.AreEqual(5, cfg.Numbers[0].InnerNumbers[1]);
            Assert.AreEqual(10, cfg.Numbers[0].InnerNumbers[2]);

        }

        [Test]
        public void TestJsonArray() {
            var json = @"{
        'numbers':[
            { 'inner-numbers': [ 1, 5, 10 ] }
        ]
    }
".Replace("'", "\"");

            var cfg = new Los(json);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            var problems = cfg.Errors();
            Assert.AreEqual(0, problems.Length);
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
