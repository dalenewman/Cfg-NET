using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Cfg.Test {
    [TestFixture]
    public class ReadMe {

        [Test]
        public void TestReadMe() {
            var xml = File.ReadAllText(@"ReadMe.xml");
            var cfg = new TestClasses.Cfg(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Problems().Count);

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

        [Test]
        public void TestReadMe2() {
            var xml = File.ReadAllText(@"ReadMe2.xml");
            var cfg = new TestClasses.Cfg(xml);

            foreach (var problem in cfg.Problems()) {
                Console.WriteLine(problem);
            }

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Problems().Count);

            //TEST SAM
            Assert.AreEqual("Sam", cfg.Servers[0].Name);
            Assert.AreEqual(2, cfg.Servers[0].Databases.Count);
            Assert.AreEqual("master", cfg.Servers[0].Databases[0].Name);
            Assert.AreEqual("msdb", cfg.Servers[0].Databases[1].Name);
            Assert.AreEqual(@"\\san\sql-backups", cfg.Servers[0].Databases[0].BackupFolder);
            Assert.AreEqual(5, cfg.Servers[0].Databases[0].BackupsToKeep);

            //TEST VINNY
            Assert.AreEqual("Vinny", cfg.Servers[1].Name);
            Assert.AreEqual(2, cfg.Servers[1].Databases.Count);
            Assert.AreEqual("master", cfg.Servers[1].Databases[0].Name);
            Assert.AreEqual(@"\\san\sql-backups\vinny\master", cfg.Servers[1].Databases[0].BackupFolder);
            Assert.AreEqual(3, cfg.Servers[1].Databases[0].BackupsToKeep);
            Assert.AreEqual("model", cfg.Servers[1].Databases[1].Name);
            Assert.AreEqual(@"\\san\sql-backups\vinny\model", cfg.Servers[1].Databases[1].BackupFolder);
            Assert.AreEqual(4, cfg.Servers[1].Databases[1].BackupsToKeep);

        }

        [Test]
        public void TestReadMeThreads1() {
            var thread1 = new Thread(TestReadMe);
            var thread2 = new Thread(TestReadMe2);
            var thread3 = new Thread(TestReadMe);
            var thread4 = new Thread(TestReadMe2);
            var thread5 = new Thread(TestReadMe);
            var thread6 = new Thread(TestReadMe2);
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();
            thread6.Start();
            TestReadMe();
            TestReadMe2();
        }

    }
}
