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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class ConfigureWithCode {

        [TestMethod]
        public void Test1() {

            var example = new Example();
            Assert.AreEqual(0, example.Errors().Length);
            Assert.IsNotNull(example.Items);

            example.Load();
            Assert.AreNotEqual(0, example.Errors().Length);
            Assert.AreEqual(1, example.Warnings().Length);
            Assert.AreEqual(2, example.Errors().Length);

            example.Items.Add(new ExampleItem { Value = 5});
            example.Items.Add(new ExampleItem { Value = 6});
            example.Load();
            Assert.AreNotEqual(0, example.Errors().Length);
            Assert.AreEqual(1, example.Warnings().Length);
            Assert.AreEqual(1, example.Errors().Length, "Only 1 error because we fixed the missing items.");

            example.Name = "I am required";
            example.Load();
            Assert.AreEqual(0, example.Errors().Length, "0 errors because we added a name.");
        }
    }

    public class Example : CfgNode {
        [Cfg(required = true)]
        public string Name { get; set; }
        
        [Cfg(required = true)]
        public List<ExampleItem> Items { get; set; }
    }

    public class ExampleItem : CfgNode {
        [Cfg(minValue = 1, maxValue = 10)]
        public int Value { get; set; }
    }
}
