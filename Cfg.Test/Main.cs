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
using System.Diagnostics;
using System.Xml.Linq;
using Cfg.Net;
using Cfg.Net.Ext;
using Cfg.Net.Parsers;
using Cfg.Test.TestClasses;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class Main {

        [Test]
        public void TestEmptyXmlCfg() {
            var cfg = new AttributeCfg(@"<cfg></cfg>");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("sites must be populated in cfg.", cfg.Errors()[0]);
        }

        [Test]
        public void TestEmptyJsonCfg() {
            var cfg = new AttributeCfg(@"{}");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("sites must be populated.", cfg.Errors()[0]);
        }

        [Test]
        public void TestGetDefaultOf() {
            var cfg = new AttributeCfg(@"<cfg></cfg>");
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.Sites);
            Assert.AreEqual(0, cfg.Sites.Count);

            var sites = new AttributeSite();
            Assert.IsNotNull(sites);
            Assert.IsNotNull(sites.Something);
        }

        [Test]
        public void TestGetJsonDefaultOf() {
            var cfg = new AttributeCfg(@"{}");
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.Sites);
            Assert.AreEqual(0, cfg.Sites.Count);

            var sites = new AttributeSite();
            Assert.IsNotNull(sites);
            Assert.IsNotNull(sites.Something);
        }

        [Test]
        public void TestNew() {
            var cfg = new AttributeCfg(@"<cfg></cfg>");
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.Sites);
            Assert.AreEqual(0, cfg.Sites.Count);
        }

        [Test]
        public void TestNewJson() {
            var cfg = new AttributeCfg(@"{}");
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.Sites);
            Assert.AreEqual(0, cfg.Sites.Count);
        }

        [Test]
        public void TestEmptySites() {
            var cfg = new AttributeCfg(@"<cfg><sites/></cfg>");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("sites must be populated in cfg.", cfg.Errors()[0]);
        }

        [Test]
        public void TestEmptyJsonSites() {
            var cfg = new AttributeCfg("{\"sites\":[]}");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("sites must be populated.", cfg.Errors()[0]);
        }

        [Test]
        public void TestInvalidProcess() {
            var cfg = new AttributeCfg(
                @"<cfg>
                    <sites>
                        <add/>
                    </sites>
                </cfg>"
            );

            var problems = cfg.Errors();

            Assert.AreEqual(2, problems.Length);

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }
            Assert.IsTrue(problems[0] == "A sites item is missing name.");
            Assert.IsTrue(problems[1] == "A sites item is missing url.");
        }

        [Test]
        public void TestInvalidSiteAttribute() {
            var cfg = new AttributeCfg(
                @"<cfg>
                    <sites>
                        <add name='dale' invalid='true' />
                    </sites>
                </cfg>".Replace("'", "\"")
            );

            var problems = cfg.Errors();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Length);
            Assert.AreEqual("A sites item contains an invalid invalid.  It may only contain: name, url, something, numeric, common.", problems[0]);
            Assert.AreEqual("A sites item is missing url.", problems[1]);

        }

        [Test]
        public void TestMultipleSites() {
            var xml = @"<cfg>
                    <sites>
                        <add name='google' url='http://www.google.com' something='&lt;'/>
                        <add name='github' url='http://www.github.com' numeric='x'/>
                        <add name='stackoverflow' url='http://www.stackoverflow.com' numeric='7' />
                        <add name='github' url='http://www.anotherGitHub.com' something='this is a duplicate!' />
                    </sites>
                </cfg>".Replace("'", "\"");

            var parser = new NanoXmlParser();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var x = parser.Parse(xml);
            stopWatch.Stop();

            Console.WriteLine("nano load ms:" + stopWatch.ElapsedMilliseconds);

            stopWatch.Restart();
            var doc = XDocument.Parse(xml);
            stopWatch.Stop();

            Console.WriteLine("xdoc parse ms:" + stopWatch.ElapsedMilliseconds);

            Assert.IsNotNull(x);
            Assert.IsNotNull(doc);

            var cfg = new AttributeCfg(xml);
            var problems = cfg.Errors();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Length);
            Assert.AreEqual("Could not set numeric to x inside sites item. Input string was not in a correct format.", problems[0]);
            Assert.AreEqual("Duplicate name value github in sites.", problems[1]);

            Assert.AreEqual(4, cfg.Sites.Count);

            Assert.AreEqual("google", cfg.Sites[0].Name);
            Assert.AreEqual("http://www.google.com", cfg.Sites[0].Url);
            Assert.AreEqual("<", cfg.Sites[0].Something);
            Assert.AreEqual(0, cfg.Sites[0].Numeric);

            Assert.AreEqual("github", cfg.Sites[1].Name);
            Assert.AreEqual("http://www.github.com", cfg.Sites[1].Url);

            Assert.AreEqual(7, cfg.Sites[2].Numeric);

        }

    }

}