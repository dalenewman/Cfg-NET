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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cfg.Net;
using Cfg.Net.Ext;
using Cfg.Net.Parsers;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ReadMe {

        [Test]
        public void TestReadMe() {

            var xml = File.ReadAllText(@"ReadMe.xml");
            var cfg = new TestClasses.Cfg(xml, new NanoXmlParser());

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Errors().Length);


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
            var cfg = new TestClasses.Cfg(xml, null);

            foreach (var problem in cfg.Errors()) {
                Console.WriteLine(problem);
            }

            //TEST FOR PROBLEMS
            Assert.AreEqual(0, cfg.Errors().Length);

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

            const string expected = @"<Cfg>
    <servers>
        <add name=""Sam"">
            <databases>
                <add name=""master"" backupstokeep=""5"" />
                <add name=""msdb"" backupstokeep=""2"" />
            </databases>
        </add>
        <add name=""Vinny"">
            <databases>
                <add name=""master"" backupfolder=""\\san\sql-backups\vinny\master"" backupstokeep=""3"" />
                <add name=""model"" backupfolder=""\\san\sql-backups\vinny\model"" />
            </databases>
        </add>
    </servers>
</Cfg>";
            var actual = cfg.Serialize();
            Assert.AreEqual(expected, actual);

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

        [Test]
        public void NewTest() {
            const string xml = @"
<cfg>
    <fruit>
        <add name='apple'>
            <colors>
                <add name='red' />
                <add name='yellow' />
                <add name='green' />
                <add name='pink' />
            </colors>
        </add>
        <add name='banana'>
            <colors>
                <add name='yellow' />
            </colors>
        </add>
    </fruit>
</cfg>
";

            var cfg = new Cfg();
            cfg.Load(xml);
            cfg.Fruit.RemoveAll(f => f.Name == "apple");
            cfg.Fruit.Add(new Fruit {
                Name = "plum",
                Colors = new List<Color> {
                    new Color { Name = "purple" }
                }
            });

            foreach (var error in cfg.Errors()) {
                Console.WriteLine(error);
            }

            Console.WriteLine(cfg.Serialize());

        }

    }

    class Cfg : CfgNode {
        [Cfg(required = true)] // THERE MUST BE SOME FRUIT!
        public List<Fruit> Fruit { get; set; }
    }

    class Fruit : CfgNode {
        [Cfg(unique = true)] // THE FRUIT MUST BE UNIQUE!
        public string Name { get; set; }
        [Cfg]
        public List<Color> Colors { get; set; }
    }

    class Color : CfgNode {
        [Cfg(domain="red,yellow,green,purple,blue,orange")]
        public string Name { get; set; }
    }
}
