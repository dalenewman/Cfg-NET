using System;
using System.Collections.Generic;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Loggers;

namespace Cfg.Test {

    [TestFixture]
    public class CustomProblems {

        [Test]
        public void TestXml() {

            var xml = @"
    <cfg>
        <connections>
            <add provider='file' file='good.txt' />
            <add provider='file' /><!-- bad, missing file -->
            <add provider='folder' folder='c:\good' />
            <add provider='folder' file='bad.txt' /><!-- bad, missing folder -->
        </connections>
    </cfg>
".Replace("'", "\"");

            var cfg = new CustomProblemCfg(xml);
            var problems = cfg.Errors();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Length);
            Assert.IsTrue(problems[0] == "file provider needs file attribute.");
            Assert.IsTrue(problems[1] == "folder provider needs folder attribute.");
        }

        [Test]
        public void TestJson() {
            var json = @"{
  'connections': [
      { 'provider': 'file', 'file': 'good.txt' },
      { 'provider': 'file' },
      { 'provider': 'folder', 'folder': 'c:\\good' },
      { 'provider': 'folder', 'file': 'bad.txt' }
    ]
}
".Replace("'", "\"");

            var cfg = new CustomProblemCfg(json);
            var problems = cfg.Errors();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Length);
            Assert.IsTrue(problems[0] == "file provider needs file attribute.");
            Assert.IsTrue(problems[1] == "folder provider needs folder attribute.");
        }


    }

    public class CustomProblemCfg : CfgNode {
        public CustomProblemCfg(string xml) {
            Load(xml);
        }

        [Cfg(required = true)]
        public List<CustomProblemConnection> Connections { get; set; }

    }

    public class CustomProblemConnection : CfgNode {

        [Cfg(required = true, domain = "file,folder,other")]
        public string Provider { get; set; }

        [Cfg()]
        public string File { get; set; }

        [Cfg()]
        public string Folder { get; set; }

        // custom validation
        protected override void Validate() {
            if (Provider == "file" && string.IsNullOrEmpty(File)) {
                Error("file provider needs file attribute.");
            } else if (Provider == "folder" && string.IsNullOrEmpty(Folder)) {
                Error("folder provider needs folder attribute.");
            }
        }

    }

}
