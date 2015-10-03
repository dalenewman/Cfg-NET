using System.Collections.Generic;
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Loggers;
using NUnit.Framework;

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

         var cfg = new CustomProblemCfg(xml, new TraceLogger());
         var problems = cfg.Errors();

         Assert.AreEqual(3, problems.Length);
         Assert.IsTrue(problems[0] == "file provider needs file attribute.");
         Assert.IsTrue(problems[1] == "I don't like c:\\good.");
         Assert.IsTrue(problems[2] == "folder provider needs folder attribute.");
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

         var cfg = new CustomProblemCfg(json, new TraceLogger());
         var problems = cfg.Errors();

         Assert.AreEqual(3, problems.Length);
         Assert.IsTrue(problems[0] == "file provider needs file attribute.");
         Assert.IsTrue(problems[1] == "I don't like c:\\good.");
         Assert.IsTrue(problems[2] == "folder provider needs folder attribute.");
      }


   }

   public class CustomProblemCfg : CfgNode {
      public CustomProblemCfg(string xml, ILogger anotherLogger) : base(anotherLogger) {
         Load(xml);
      }

      [Cfg(required = true)]
      public List<CustomProblemConnection> Connections { get; set; }

   }

   public class CustomProblemConnection : CfgNode {

      [Cfg(required = true, domain = "file,folder,other")]
      public string Provider { get; set; }

      [Cfg]
      public string File { get; set; }
      string folder;

      [Cfg]
      public string Folder {
         get {
            return folder;
         }

         set {
            if (value != null) {
               Error("I don't like {0}.", value);
            }
            folder = value;
         }
      }

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
