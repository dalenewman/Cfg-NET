#region license
// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2017 Dale Newman
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
#endregion
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
