// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2018 Dale Newman
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

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {
   [TestClass]
   public class ReadMeJson {

      [TestMethod]
      public void TestReadMe() {
         var json = File.ReadAllText(@"ReadMe.json");

         var cfg = new TestClasses.Cfg(json, null);

         foreach (var problem in cfg.Errors()) {
            Console.WriteLine(problem);
         }

         //TEST FOR PROBLEMS
         Assert.AreEqual(0, cfg.Errors().Length);
         Assert.AreEqual(2, cfg.Servers.Count);

         //TEST Id
         Assert.AreEqual(1, cfg.Id);

         //TEST GANDALF
         Assert.AreEqual("Gandalf", cfg.Servers[0].Name);
         Assert.AreEqual(1, cfg.Servers[0].Databases.Count);
         Assert.AreEqual("master", cfg.Servers[0].Databases[0].Name);
         Assert.AreEqual(@"\\san\sql-backups", cfg.Servers[0].Databases[0].BackupFolder);
         Assert.AreEqual(6, cfg.Servers[0].Databases[0].BackupsToKeep);

         //TEST SARUMAN
         Assert.AreEqual("Saruman", cfg.Servers[1].Name);
         Assert.AreEqual(2, cfg.Servers[1].Databases.Count);
         Assert.AreEqual("master", cfg.Servers[1].Databases[0].Name);
         Assert.AreEqual(@"\\san\sql-backups\saruman\master", cfg.Servers[1].Databases[0].BackupFolder);
         Assert.AreEqual(8, cfg.Servers[1].Databases[0].BackupsToKeep);
         Assert.AreEqual("model", cfg.Servers[1].Databases[1].Name);
         Assert.AreEqual(@"\\san\sql-backups\saruman\model", cfg.Servers[1].Databases[1].BackupFolder);
         Assert.AreEqual(4, cfg.Servers[1].Databases[1].BackupsToKeep);

      }
   }
}
