using System;
using System.Diagnostics;
using System.Xml.Linq;
using Cfg.Test.TestClasses;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class MethodBased {

        [Test]
        public void TestEmptyCfg() {
            var cfg = new MethodCfg();
            cfg.Load(@"<cfg></cfg>");
            Assert.AreEqual(1, cfg.Problems().Count);
            Assert.AreEqual("The 'cfg' element is missing a 'sites' element.", cfg.Problems()[0]);
        }

        [Test]
        public void TestEmptySites() {
            var cfg = new MethodCfg();
            cfg.Load(@"<cfg><sites/></cfg>");
            Assert.AreEqual(1, cfg.Problems().Count);
            Assert.AreEqual("A 'sites' element is missing an 'add' element.", cfg.Problems()[0]);
        }

        [Test]
        public void TestInvalidProcess() {
            var cfg = new MethodCfg();
            cfg.Load(
                @"<cfg>
                    <sites>
                        <add/>
                    </sites>
                </cfg>"
            );

            var problems = cfg.Problems();

            Assert.AreEqual(2, problems.Count);

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }
            Assert.IsTrue(problems.Contains("A 'sites' 'add' element is missing a 'name' attribute."));
            Assert.IsTrue(problems.Contains("A 'sites' 'add' element is missing a 'url' attribute."));
        }

        [Test]
        public void TestInvalidSiteAttribute() {
            var cfg = new MethodCfg();
            cfg.Load(
                @"<cfg>
                    <sites>
                        <add name='dale' invalid='true' />
                    </sites>
                </cfg>".Replace("'", "\"")
            );

            var problems = cfg.Problems();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Count);
            Assert.AreEqual("A 'sites' 'add' element contains an invalid 'invalid' attribute.  Valid attributes are: name, url, something, numeric.", problems[0]);
            Assert.AreEqual("A 'sites' 'add' element is missing a 'url' attribute.", problems[1]);

        }

        [Test]
        public void TestMultipleSites() {
            var xml = @"<cfg>
                    <sites>
                        <add name='google' url='http://www.google.com' something='&lt;'/>
                        <add name='github' url='http://www.github.com' numeric='x'/>
                        <add name='stackoverflow' url='http://www.stackoverflow.com' numeric='7' />
                    </sites>
                </cfg>".Replace("'", "\"");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var cfg = new MethodCfg();
            cfg.Load(xml);
            stopWatch.Stop();

            Console.WriteLine("cfg.net load ms:" + stopWatch.ElapsedMilliseconds);

            stopWatch.Restart();
            var doc = XDocument.Parse(xml);
            stopWatch.Stop();

            Console.WriteLine("xdoc parse ms:" + stopWatch.ElapsedMilliseconds);

            Assert.IsNotNull(doc);

            var problems = cfg.Problems();
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("Could not set 'numeric' to 'x' inside 'sites' 'add'. Input string was not in a correct format.", problems[0]);

            Assert.AreEqual(3, cfg.Count("sites"));

            Assert.AreEqual("google", cfg["sites", 0]["name"].Attributes.value);
            Assert.AreEqual("http://www.google.com", cfg["sites", 0]["url"].Attributes.value);
            Assert.AreEqual("<", cfg["sites", 0]["something"].Attributes.value);
            Assert.AreEqual(0, cfg["sites", 0]["numeric"].Attributes.value);
            Assert.IsTrue(cfg["sites", 0]["numeric"].Attributes.value is int);

            Assert.AreEqual("github", cfg["sites", 1]["name"].Attributes.value);
            Assert.AreEqual("http://www.github.com", cfg["sites", 1]["url"].Attributes.value);

            Assert.AreEqual(7, cfg["sites", 2]["numeric"].Attributes.value);

        }

        [Test]
        public void TestSharedProperty()
        {
            var cfg = new MethodCfg();
            cfg.Load(@"<cfg>
                    <sites common='colts'>
                        <add name='google' url='http://www.google.com' something='&lt;'/>
                        <add name='github' url='http://www.github.com' numeric='5'/>
                        <add name='stackoverflow' url='http://www.stackoverflow.com' numeric='7' />
                    </sites>
                </cfg>".Replace("'", "\""));

            Assert.AreEqual(0, cfg.Problems().Count);
            Assert.AreEqual("colts", cfg["sites", 0]["common"].Attributes.value);
            Assert.AreEqual("colts", cfg["sites", 1]["common"].Attributes.value);
            Assert.AreEqual("colts", cfg["sites", 2]["common"].Attributes.value);

        }

    }
}