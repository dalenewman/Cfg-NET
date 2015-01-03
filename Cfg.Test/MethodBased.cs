using System;
using System.Diagnostics;
using System.Xml.Linq;
using Cfg.Test.TestClasses;
using NUnit.Framework;
using Transformalize.Libs.Cfg;

namespace Cfg.Test {

    [TestFixture]
    public class MethodBased {

        [Test]
        public void TestEmptyCfg() {
            var root = new MethodCfg().Load(new NanoXmlDocument(@"<cfg></cfg>").RootNode);
            Assert.AreEqual(1, root.AllProblems().Count);
            Assert.AreEqual("The 'cfg' element is missing a 'sites' element.", root.AllProblems()[0]);
        }

        [Test]
        public void TestEmptySites() {
            var node = new MethodCfg().Load(new NanoXmlDocument(@"<cfg><sites/></cfg>").RootNode);
            Assert.AreEqual(1, node.AllProblems().Count);
            Assert.AreEqual("A 'sites' element is missing an 'add' element.", node.AllProblems()[0]);
        }

        [Test]
        public void TestInvalidProcess() {
            var node = new MethodCfg().Load(new NanoXmlDocument(
                @"<cfg>
                    <sites>
                        <add/>
                    </sites>
                </cfg>"
                ).RootNode);

            var problems = node.AllProblems();

            Assert.AreEqual(2, problems.Count);

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }
            Assert.IsTrue(problems.Contains("A 'sites' 'add' element is missing a 'name' attribute."));
            Assert.IsTrue(problems.Contains("A 'sites' 'add' element is missing a 'url' attribute."));
        }

        [Test]
        public void TestInvalidSiteAttribute() {
            var cfg = new NanoXmlDocument(
                @"<cfg>
                    <sites>
                        <add name='dale' invalid='true' />
                    </sites>
                </cfg>".Replace("'", "\"")
                ).RootNode;
            var node = new MethodCfg().Load(cfg);

            var problems = node.AllProblems();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Count);
            Assert.AreEqual(
                "A 'sites' 'add' element contains an invalid 'invalid' attribute.  Valid attributes are: name, url, something, numeric.",
                problems[0]);
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
            var node = new MethodCfg().Load(new NanoXmlDocument(xml).RootNode);
            stopWatch.Stop();

            Console.WriteLine("cfg.net load ms:" + stopWatch.ElapsedMilliseconds);

            stopWatch.Restart();
            var doc = XDocument.Parse(xml);
            stopWatch.Stop();

            Console.WriteLine("xdoc parse ms:" + stopWatch.ElapsedMilliseconds);

            Assert.IsNotNull(doc);

            var problems = node.AllProblems();
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("Could not set 'numeric' to 'x' inside 'sites' 'add'. Input string was not in a correct format.", problems[0]);

            Assert.AreEqual(3, node.Count("sites"));

            Assert.AreEqual("google", node["sites", 0]["name"].Value);
            Assert.AreEqual("http://www.google.com", node["sites", 0]["url"].Value);
            Assert.AreEqual("<", node["sites", 0]["something"].Value);
            Assert.AreEqual(0, node["sites", 0]["numeric"].Value);
            Assert.IsTrue(node["sites", 0]["numeric"].Value is int);

            Assert.AreEqual("github", node["sites", 1]["name"].Value);
            Assert.AreEqual("http://www.github.com", node["sites", 1]["url"].Value);

            Assert.AreEqual(7, node["sites", 2]["numeric"].Value);

        }
    }
}