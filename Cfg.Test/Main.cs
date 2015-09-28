using System;
using System.Diagnostics;
using System.Xml.Linq;
using Cfg.Test.TestClasses;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class Main {

        [Test]
        public void TestEmptyXmlCfg() {
            var cfg = new AttributeCfg(@"<cfg></cfg>");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("The 'cfg' element is missing a 'sites' element.", cfg.Errors()[0]);
        }

        [Test]
        public void TestEmptyJsonCfg() {
            var cfg = new AttributeCfg(@"{}");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("The root element is missing a 'sites' element.", cfg.Errors()[0]);
        }

        [Test]
        public void TestGetDefaultOf() {
            var cfg = new AttributeCfg(@"<cfg></cfg>");
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.Sites);
            Assert.AreEqual(0, cfg.Sites.Count);

            var sites = cfg.GetDefaultOf<AttributeSite>();
            Assert.IsNotNull(sites);
            Assert.IsNotNull(sites.Something);
        }

        [Test]
        public void TestGetJsonDefaultOf() {
            var cfg = new AttributeCfg(@"{}");
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.Sites);
            Assert.AreEqual(0, cfg.Sites.Count);

            var sites = cfg.GetDefaultOf<AttributeSite>();
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
            Assert.AreEqual("A 'sites' element is missing a child element.", cfg.Errors()[0]);
        }

        [Test]
        public void TestEmptyJsonSites() {
            var cfg = new AttributeCfg("{\"sites\":[]}");
            Assert.AreEqual(1, cfg.Errors().Length);
            Assert.AreEqual("A 'sites' element is missing a child element.", cfg.Errors()[0]);
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
            Assert.IsTrue(problems[0] == "A 'sites' 'add' element is missing a 'name' attribute.");
            Assert.IsTrue(problems[1] == "A 'sites' 'add' element is missing a 'url' attribute.");
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
            Assert.AreEqual("A 'sites' 'add' element contains an invalid 'invalid' attribute.  Valid attributes are: name, url, something, numeric, common.", problems[0]);
            Assert.AreEqual("A 'sites' 'add' element is missing a 'url' attribute.", problems[1]);

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

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var cfg = new AttributeCfg(xml);
            stopWatch.Stop();

            Console.WriteLine("cfg.net load ms:" + stopWatch.ElapsedMilliseconds);

            stopWatch.Restart();
            var doc = XDocument.Parse(xml);
            stopWatch.Stop();

            Console.WriteLine("xdoc parse ms:" + stopWatch.ElapsedMilliseconds);

            Assert.IsNotNull(doc);

            var problems = cfg.Errors();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Length);
            Assert.AreEqual("Could not set 'numeric' to 'x' inside 'sites' 'add'. Input string was not in a correct format.", problems[0]);
            Assert.AreEqual("Duplicate 'name' value 'github' in 'sites'.", problems[1]);

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