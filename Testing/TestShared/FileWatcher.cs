// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2019 Dale Newman
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

using System.Xml;
using Cfg.Net.Readers.FileSystemWatcherReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {
    [TestClass]
    public class FileWatcher {

        [TestMethod]
        //[Ignore("integration test that affects other tests")]
        public void TestFileWatcher() {

         using(var watcher = new FileSystemWatcherReader()) {
            var cfg = new TestClasses.Cfg("ReadMe.xml", watcher);

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Errors().Length);
            Assert.AreEqual(1, cfg.Id);

            var fullName = new FileFinder().Find("ReadMe.xml", new TraceLogger());
            var doc = new XmlDocument();
            doc.Load(fullName);

            var root = doc.SelectSingleNode("/cfg");
            root.Attributes["id"].Value = "2";
            doc.Save(fullName);
            System.Threading.Thread.Sleep(500); // give file system watcher some time

            Assert.AreEqual(0, cfg.Errors().Length);
            Assert.AreEqual(2, cfg.Id);

            root.Attributes["id"].Value = "3";
            doc.Save(fullName);
            System.Threading.Thread.Sleep(500);

            Assert.AreEqual(0, cfg.Errors().Length);
            Assert.AreEqual(3, cfg.Id);

            root.Attributes["id"].Value = "1";
            doc.Save(fullName);

         }
      }
   }
}
