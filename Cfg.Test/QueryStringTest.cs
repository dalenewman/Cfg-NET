using System.Collections.Generic;
using Cfg.Net.Loggers;
using Cfg.Net.Reader;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class QueryStringTest {

        [Test]
        public void TestAbsoluteFile() {
            const string resource = @"C:\Code\Cfg.Net\Cfg.Test\shorthand.xml?mode=init&title=hello%20world";
            var expected = new Dictionary<string,string> { {"mode","init"}, {"title","hello world"}};
            var actual = new FileReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestRelativeFileWithRepeatingTitleParameter() {
            const string resource = @"shorthand.xml?mode=init&title=hello%20world&title=no";
            var expected = new Dictionary<string, string> { { "mode", "init" }, { "title", "hello world,no" } };
            var actual = new FileReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestFileWithInvalidQueryString() {
            const string resource = @"shorthand.xml?mode=";
            var expected = new Dictionary<string, string> { { "mode", string.Empty }};
            var actual = new FileReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestUrl() {
            const string resource = @"http://config.mwf.local/NorthWind.xml?mode=init&title=hello%20world";
            var expected = new Dictionary<string, string> { { "mode", "init" }, { "title", "hello world" } };
            var actual = new WebReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestJustQuestionMark() {
            const string resource = @"http://config.mwf.local/NorthWind.xml?";
            var expected = new Dictionary<string, string>();
            var actual = new WebReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestNothing() {
            const string resource = @"http://config.mwf.local/NorthWind.xml";
            var expected = new Dictionary<string, string>();
            var actual = new WebReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestExAndWhy() {
            const string resource = @"http://config.mwf.local/NorthWind.xml?x&y";
            var expected = new Dictionary<string, string> { { "x", string.Empty }, { "y", string.Empty } };
            var actual = new WebReader().Read(resource, new TraceLogger()).Parameters;
            Assert.AreEqual(expected, actual);
        }
    }
}
