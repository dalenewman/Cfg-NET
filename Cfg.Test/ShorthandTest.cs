using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Shorthand;

namespace Cfg.Test {

    [TestFixture]
    public class ShorthandTest {

        [Test]
        public void TestShorthandConfiguration() {
            var root = new ShorthandRoot(File.ReadAllText(@"shorthand.xml"));

            foreach (var error in root.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, root.Errors().Count());
            Assert.AreEqual(3, root.Signatures.Count);
            Assert.AreEqual(1, root.Targets.Count);
            Assert.AreEqual(5, root.Methods.Count);
        }

        [Test]
        public void TestSampleConfiguration() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='left' t='left(1)' />
                        <add name='right' t='right(2)' />
                        <add name='padleft' t='padleft(10,0)' />
                        <add name='padright' t='padright(10).left(10)' />
                    </fields>
                </cfg>
            ";

            var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            Assert.AreEqual(4, sample.Fields.Count());
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
        }

        [Test]
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

            var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

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

        [Test]
        public void TestSingleParameterShouldNotConsiderUnescapedSplitter() {
            const string xml = @"
                <cfg>
                    <fields>
                        <add name='javascript' t='javascript(x === y ? x : ,)' />
                    </fields>
                </cfg>";

            var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

            foreach (var error in sample.Errors()) {
                Console.WriteLine(error);
            }

            Assert.AreEqual(0, sample.Errors().Count());
            var transform = sample.Fields[0].Transforms[0];
            Assert.AreEqual("javascript", transform.Method);
            Assert.AreEqual("x === y ? x : ,", transform.Script);
        }

      [Test]
      public void TestEscapedComma() {
         const string xml = @"
                <cfg>
                    <fields>
                        <add name='name' 
                             t='padleft(10,\,)' />
                    </fields>
                </cfg>";

         var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

         foreach (var error in sample.Errors()) {
            Console.WriteLine(error);
         }

         Assert.AreEqual(0, sample.Errors().Count());
         var transform = sample.Fields[0].Transforms[0];
         Assert.AreEqual("padleft", transform.Method);
         Assert.AreEqual(10, transform.TotalWidth);
         Assert.AreEqual(",", transform.PaddingChar);
      }

      [Test]
      public void TestSingleParameterThatEndsWithParenthesis() {
         const string xml = @"
                <cfg>
                    <fields>
                        <add name='javascript' t='javascript(OrderDetailsQuantity * (OrderDetailsUnitPrice * (1-OrderDetailsDiscount)))' />
                    </fields>
                </cfg>";

         var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

         foreach (var error in sample.Errors()) {
            Console.WriteLine(error);
         }

         Assert.AreEqual(0, sample.Errors().Count());
         var transform = sample.Fields[0].Transforms[0];
         Assert.AreEqual("javascript", transform.Method);
         Assert.AreEqual("OrderDetailsQuantity * (OrderDetailsUnitPrice * (1-OrderDetailsDiscount))", transform.Script);
      }

      [Test]
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

            var sample = new ShTestCfg(xml, File.ReadAllText(@"shorthand.xml"));

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
        public ShTestCfg(string cfg, string shorthand) {
            LoadShorthand(shorthand);
            Load(cfg);
        }

        [Cfg(required = true)]
        public List<ShTestField> Fields { get; set; }
    }

    public class ShTestField : CfgNode {

        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg(required = true, shorthand = true)]
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

        [Cfg(value="")]
        public string Script { get; set; }
    }
}
