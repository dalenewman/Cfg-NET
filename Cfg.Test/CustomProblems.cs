using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

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
            var problems = cfg.Problems();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Count);
            Assert.IsTrue(problems.Contains("file provider needs file attribute."));
            Assert.IsTrue(problems.Contains("folder provider needs folder attribute."));
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
            var problems = cfg.Problems();

            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(2, problems.Count);
            Assert.IsTrue(problems.Contains("file provider needs file attribute."));
            Assert.IsTrue(problems.Contains("folder provider needs folder attribute."));
        }


    }

    public sealed class CustomProblemCfg : CfgNode {
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
                AddProblem("file provider needs file attribute.");
            } else if (Provider == "folder" && string.IsNullOrEmpty(Folder)) {
                AddProblem("folder provider needs folder attribute.");
            }
        }

    }

}
