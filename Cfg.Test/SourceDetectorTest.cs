using Cfg.Net;
using Cfg.Net.Reader;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class SourceDetectorTest {

        [Test]
        public void TestAbsoluteFile() {
            const string resource = @"C:\Code\Cfg.Net\Cfg.Test\shorthand.xml";
            const Source expected = Source.File;
            var actual = new SourceDetector().Detect(resource, new MyLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestRelativeFile() {
            const string resource = @"shorthand.xml";
            const Source expected = Source.File;
            var actual = new SourceDetector().Detect(resource, new MyLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestUrl() {
            const string resource = @"http://www.somewhere.com/shorthand.xml";
            const Source expected = Source.Url;
            var actual = new SourceDetector().Detect(resource, new MyLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestXml() {
            const string resource = @"<cfg><processes></processes></cfg>";
            const Source expected = Source.Xml;
            var actual = new SourceDetector().Detect(resource, new MyLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestJson() {
            const string resource = @"{processes:[{""one"":1}]}";
            const Source expected = Source.Json;
            var actual = new SourceDetector().Detect(resource, new MyLogger());
            Assert.AreEqual(expected, actual);
        }



        [Test]
        public void TestSomethingBad() {
            const string resource = "alkshg";
            const Source expected = Source.Error;
            var actual = new SourceDetector().Detect(resource, new MyLogger());
            Assert.AreEqual(expected, actual);
        }
    }
}
