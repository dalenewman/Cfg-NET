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
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Ext;
using Cfg.Net.Loggers;
using Cfg.Net.Reader;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class SourceDetectorTest {

        [Test]
        public void TestAbsoluteFile() {
            const string resource = @"C:\Code\Cfg.Net\Cfg.Test\shorthand.xml";
            const Source expected = Source.File;
            var actual = new SourceDetector().Detect(resource, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestRelativeFile() {
            const string resource = @"shorthand.xml";
            const Source expected = Source.File;
            var actual = new SourceDetector().Detect(resource, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestUrl() {
            const string resource = @"http://www.somewhere.com/shorthand.xml";
            const Source expected = Source.Url;
            var actual = new SourceDetector().Detect(resource, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestXml() {
            const string resource = @"<cfg><processes></processes></cfg>";
            const Source expected = Source.Xml;
            var actual = new SourceDetector().Detect(resource, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestJson() {
            const string resource = @"{processes:[{""one"":1}]}";
            const Source expected = Source.Json;
            var actual = new SourceDetector().Detect(resource, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }



        [Test]
        public void TestSomethingBad() {
            const string resource = "alkshg";
            const Source expected = Source.Error;
            var actual = new SourceDetector().Detect(resource, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }
    }
}
