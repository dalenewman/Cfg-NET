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
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

   [TestClass]
   public class Types {

      [TestMethod]
      public void TestWrongTypes() {
         var xml = @"
    <test>
        <things>
            <add IntValue='3' ShortValue='3' />
            <add IntValue='x' ShortValue='40000' />
        </things>
    </test>
".Replace("'", "\"");

         var cfg = new TestTypes(xml);

         foreach (var problem in cfg.Errors()) {
            Console.WriteLine(problem);
         }

         var problems = cfg.Errors();
         Assert.AreEqual(2, problems.Length);
         Assert.AreEqual("Could not set IntValue to x inside things item. Input string was not in a correct format.", problems[0]);
         Assert.AreEqual("Could not set ShortValue to 40000 inside things item. Value was either too large or too small for an Int16.", problems[1]);

         Assert.AreEqual(3, cfg.Things.First().IntValue);
         Assert.AreEqual((short)3, cfg.Things.First().ShortValue);

         Assert.AreEqual(default, cfg.Things.Last().IntValue);
         Assert.AreEqual(default, cfg.Things.Last().ShortValue);

      }

      [TestMethod]
      public void TestWrongTypesDisabled() {
         var xml = @"
    <test>
        <things>
            <add IntValue='3' ShortValue='3' />
            <add IntValue='x' ShortValue='40000' />
        </things>
    </test>
".Replace("'", "\"");

         var cfg = new TestTypes(xml, enabled:false);

         foreach (var problem in cfg.Errors()) {
            Console.WriteLine(problem);
         }

         var problems = cfg.Errors();
         Assert.AreEqual(2, problems.Length);
         Assert.AreEqual("Could not set IntValue to x inside things item. Input string was not in a correct format.", problems[0]);
         Assert.AreEqual("Could not set ShortValue to 40000 inside things item. Value was either too large or too small for an Int16.", problems[1]);

         Assert.AreEqual(3, cfg.Things.First().IntValue);
         Assert.AreEqual((short)3, cfg.Things.First().ShortValue);

         Assert.AreEqual(default, cfg.Things.Last().IntValue);
         Assert.AreEqual(default, cfg.Things.Last().ShortValue);

      }


      public class TestTypes : CfgNode {
         [Cfg()]
         public List<TypeThing> Things { get; set; }

         public TestTypes(string xml, bool enabled = true) {
            Load(xml, enabled: true);
         }
      }

      public class TypeThing : CfgNode {
         [Cfg()]
         public int IntValue { get; set; }

         [Cfg()]
         public short ShortValue { get; set; }
      }

   }
}
