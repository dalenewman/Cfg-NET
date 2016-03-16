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
            var actual = new Dictionary<string,string>();
            var expected = new Dictionary<string,string> { {"mode","init"}, {"title","hello world"}};
            new FileReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestRelativeFileWithRepeatingTitleParameter() {
            const string resource = @"shorthand.xml?mode=init&title=hello%20world&title=no";
            var actual = new Dictionary<string,string>();
            var expected = new Dictionary<string, string> { { "mode", "init" }, { "title", "hello world,no" } };
            new FileReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestFileWithInvalidQueryString() {
            const string resource = @"shorthand.xml?mode=";
            var actual = new Dictionary<string, string>();
            var expected = new Dictionary<string, string> { { "mode", string.Empty }};
            new FileReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestUrl() {
            const string resource = @"http://config.mwf.local/NorthWind.xml?mode=init&title=hello%20world";
            var actual = new Dictionary<string, string>();
            var expected = new Dictionary<string, string> { { "mode", "init" }, { "title", "hello world" } };
            new WebReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestJustQuestionMark() {
            const string resource = @"http://config.mwf.local/NorthWind.xml?";
            var actual = new Dictionary<string, string>();
            var expected = new Dictionary<string, string>();
            new WebReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestNothing() {
            const string resource = @"http://config.mwf.local/NorthWind.xml";
            var actual = new Dictionary<string, string>();
            var expected = new Dictionary<string, string>();
            new WebReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("because web server is used")]
        public void TestExAndWhy() {
            var actual = new Dictionary<string, string>();
            const string resource = @"http://config.mwf.local/NorthWind.xml?x&y";
            var expected = new Dictionary<string, string> { { "x", string.Empty }, { "y", string.Empty } };
           new WebReader().Read(resource, actual, new TraceLogger());
            Assert.AreEqual(expected, actual);
        }
    }
}
