using System;
using System.IO;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net.Loggers;

namespace Cfg.Test {
    [TestFixture]
    public class ReadMeJson {

        [Test]
        public void TestReadMe() {
            var json = File.ReadAllText(@"ReadMe.json");

            var cfg = new TestClasses.Cfg(json, null);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Errors().Length);
            Assert.AreEqual(2, cfg.Servers.Count);

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
