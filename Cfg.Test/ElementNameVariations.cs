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
using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using NUnit.Framework;

namespace Cfg.Test {
    [TestFixture]
    public class ElementNameVariations {

        [Test]
        public void SlugsXml() {
            const string xml = @"<cfg><big-values><add big-value=""99999999"" /></big-values></cfg>";
            var cfg = new Big(xml);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        [Test]
        public void SlugsJson() {
            var json = @"{'big-values':[ { 'big-value':99999999 }]}".Replace("'", "\"");
            var cfg = new Big(json);
            var problems = cfg.Errors();

            foreach (var problem in problems)
            {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void CamelCaseXml() {
            const string xml = @"<cfg><bigValues><add bigValue=""99999999"" /></bigValues></cfg>";
            var cfg = new Big(xml);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void CamelCaseJson() {
            var json = @"{ 'bigValues': [ { 'bigValue':99999999 }]}".Replace("'", "\"");
            var cfg = new Big(json);

            var problems = cfg.Errors();
            foreach (var problem in problems)
            {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void TitleCaseXml() {
            const string xml = @"<cfg><BigValues><add BigValue=""99999999"" /></BigValues></cfg>";
            var cfg = new Big(xml);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void TitleCaseJson() {
            var json = @"{ 'BigValues': [ { 'BigValue':99999999 }]}".Replace("'", "\"");
            var cfg = new Big(json);
            var problems = cfg.Errors();

            Assert.AreEqual(0, problems.Length);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public class Big : CfgNode {
            public Big(string xml) {
                Load(xml);
            }

            [Cfg(required = true)]
            public List<CfgBigValue> BigValues { get; set; }
        }
    }

    public class CfgBigValue : CfgNode {
        [Cfg(value = (long)0)]
        public long BigValue { get; set; }
    }
}
