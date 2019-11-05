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
using System.IO;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Cfg.Net.Reader;
using Cfg.Net.Shorthand;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class ShorthandTest {

        [TestMethod]
        public void TestShorthandConfiguration() {
            var root = new ShorthandRoot(File.ReadAllText(@"shorthand.xml"));

            foreach (var error in root.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, root.Errors().Count());
            Assert.AreEqual(5, root.Signatures.Count);
            Assert.AreEqual(7, root.Methods.Count);
        }

        [TestMethod]
        public void TestSampleConfiguration() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='left' t='left(1)' />
                        <add name='right' t='right(2)' />
                        <add name='padleft' t='padleft(10,0)' />
                        <add name='padright' t='copy(x,y).padright(10).left(10).balls()' />
                        <add name='in' t='copy().in(1,2)' />
                    </fields>
                </cfg>
            ";

            var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());
            var sample = new ShTestCfg(xml, new ShorthandCustomizer(sh, new[] { "fields" }, "t", "transforms", "method"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            Assert.AreEqual(1, sample.Warnings().Count());
            Assert.AreEqual(5, sample.Fields.Count());
            Assert.AreEqual("left(1)", sample.Fields[0].T);
            Assert.AreEqual(1, sample.Fields[0].Transforms.Count);

            var left = sample.Fields[0].Transforms.First();
            Assert.AreEqual("left", left.Method);
            Assert.AreEqual(1, left.Length);

            Assert.AreEqual(2, sample.Fields[3].Transforms.Count);

            var first = sample.Fields[3].Transforms.First();
            Assert.AreEqual("padright", first.Method);
            Assert.AreEqual(10, first.TotalWidth);
            Assert.AreEqual("0", first.PaddingChar);

            var last = sample.Fields[3].Transforms.Last();
            Assert.AreEqual("left", last.Method);
            Assert.AreEqual(10, last.Length);

            Assert.AreEqual("in", sample.Fields[4].Transforms.First().Method);
            Assert.AreEqual("1,2", sample.Fields[4].Transforms.First().Domain);
        }

        [TestMethod]
        public void TestNamedParameters() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='left' t='left(length:1)' />
                        <add name='right' t='right(length:2)' />
                        <add name='padleft' t='padleft(total-width:10,padding-char:0)' />
                        <add name='padright' t='padright(totalwidth:10).left(length:10)' />
                    </fields>
                </cfg>
            ";

            var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());
            var sample = new ShTestCfg(xml, new ShorthandCustomizer(sh, new[] { "fields" }, "t", "transforms", "method"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            Assert.AreEqual(4, sample.Fields.Count());
            Assert.AreEqual("left(length:1)", sample.Fields[0].T);
            Assert.AreEqual(1, sample.Fields[0].Transforms.Count);

            var left = sample.Fields[0].Transforms.First();
            Assert.AreEqual("left", left.Method);
            Assert.AreEqual(1, left.Length);

            Assert.AreEqual(2, sample.Fields[3].Transforms.Count);

            var first = sample.Fields[3].Transforms.First();
            Assert.AreEqual("padright", first.Method);
            Assert.AreEqual(10, first.TotalWidth);
            Assert.AreEqual("0", first.PaddingChar);

            var last = sample.Fields[3].Transforms.Last();
            Assert.AreEqual("left", last.Method);
            Assert.AreEqual(10, last.Length);
        }

        [TestMethod]
        public void TestSingleParameterShouldNotConsiderUnescapedSplitter() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='javascript' t='javascript(x === y ? x : ,)' />
                    </fields>
                </cfg>";

            var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());
            var sample = new ShTestCfg(xml, new ShorthandCustomizer(sh, new[] { "fields" }, "t", "transforms", "method"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            var transform = sample.Fields[0].Transforms[0];
            Assert.AreEqual("javascript", transform.Method);
            Assert.AreEqual("x === y ? x : ,", transform.Script);
        }

        [TestMethod]
        public void TestEscapedComma() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='name' 
                             t='padleft(10,\,)' />
                    </fields>
                </cfg>";

            var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());
            var sample = new ShTestCfg(xml, new ShorthandCustomizer(sh, new[] { "fields" }, "t", "transforms", "method"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            var transform = sample.Fields[0].Transforms[0];
            Assert.AreEqual("padleft", transform.Method);
            Assert.AreEqual(10, transform.TotalWidth);
            Assert.AreEqual(",", transform.PaddingChar);
        }

        [TestMethod]
        public void TestSingleParameterThatEndsWithParenthesis() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='javascript' t='javascript(OrderDetailsQuantity * (OrderDetailsUnitPrice * (1-OrderDetailsDiscount)))' />
                    </fields>
                </cfg>";

            var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());
            var sample = new ShTestCfg(xml, new ShorthandCustomizer(sh, new[] { "fields" }, "t", "transforms", "method"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            var transform = sample.Fields[0].Transforms[0];
            Assert.AreEqual("javascript", transform.Method);
            Assert.AreEqual("OrderDetailsQuantity * (OrderDetailsUnitPrice * (1-OrderDetailsDiscount))", transform.Script);
        }

        [TestMethod]
        public void TestMixedParameters() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='padleft' t='padleft(10,padding-char:0).left(10)' />
                        <add name='padright' t='padright(totalwidth:10, )' />
                        <add name='padleft_outoforder' t='left(5).padleft(padding-char:0,10)' />
                    </fields>
                </cfg>
            ";

            var sh = new ShorthandRoot(@"shorthand.xml", new FileReader());
            var sample = new ShTestCfg(xml, new ShorthandCustomizer(sh, new[] { "fields" }, "t", "transforms", "method"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            Assert.AreEqual(3, sample.Fields.Count());

            var first = sample.Fields.First();

            Assert.AreEqual("padleft(10,padding-char:0).left(10)", first.T);
            Assert.AreEqual(2, first.Transforms.Count);
            Assert.AreEqual("padleft", first.Transforms.First().Method);
            Assert.AreEqual(10, first.Transforms.First().TotalWidth);
            Assert.AreEqual("0", first.Transforms.First().PaddingChar);

            var second = sample.Fields.Skip(1).First();

            Assert.AreEqual("padright(totalwidth:10, )", second.T);
            Assert.AreEqual(1, second.Transforms.Count);
            Assert.AreEqual("padright", second.Transforms.First().Method);
            Assert.AreEqual(10, second.Transforms.First().TotalWidth);
            Assert.AreEqual(" ", second.Transforms.First().PaddingChar);

            var third = sample.Fields.Last();

            Assert.AreEqual("left(5).padleft(padding-char:0,10)", third.T);
            Assert.AreEqual(2, third.Transforms.Count);
            Assert.AreEqual("left", third.Transforms.First().Method);
            Assert.AreEqual(5, third.Transforms.First().Length);

            Assert.AreEqual(10, third.Transforms.Last().TotalWidth);
            Assert.AreEqual("0", third.Transforms.Last().PaddingChar);

        }

    }

    public class ShTestCfg : CfgNode {
        public ShTestCfg(string cfg, params IDependency[] dependencies) : base(dependencies) {
            Load(cfg);
        }

        [Cfg(required = true)]
        public List<ShTestField> Fields { get; set; }
    }

    public class ShTestField : CfgNode {

        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public string T { get; set; }

        [Cfg]
        public List<ShTestTransform> Transforms { get; set; }
    }

    public class ShTestTransform : CfgNode {
        [Cfg(required = true)]
        public string Method { get; set; }

        [Cfg]
        public int TotalWidth { get; set; }
        [Cfg]
        public string PaddingChar { get; set; }
        [Cfg]
        public int Length { get; set; }

        [Cfg(value = "")]
        public string Script { get; set; }

        [Cfg]
        public string Domain { get; set; }

        [Cfg]
        public string Parameter { get; set; }
    }
}
