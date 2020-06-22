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

using System.Collections.Generic;
using Cfg.Net;
using Cfg.Net.Contracts;

namespace UnitTest.TestClasses {

   public sealed class Cfg : CfgNode {

      [Cfg]
      public int Id { get; set; }

      [Cfg(required = true)]
      public List<CfgServer> Servers { get; set; }

      public Cfg(string cfg) {
         Load(cfg);
      }

      public Cfg(string cfg, IParser parser) : base(parser) {
         Load(cfg);
      }

      public Cfg(string fileName, IActiveReader activeReader) : base(activeReader) {
         Load(fileName);
      }

      protected override void OnChange(object sender, CfgEventArgs e) {
         System.Console.WriteLine($"I am reloading {e.Source}");
         base.OnChange(sender, e);
      }

   }
}