using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {
    [TestFixture]
    public class ElementNameVariations {

        [Test]
        public void SlugsXml() {
            const string xml = @"<cfg><big-values><add big-value=""99999999"" /></big-values></cfg>";
            var cfg = new Big(xml);
            var problems = cfg.Problems();

            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        [Test]
        public void SlugsJson() {
            var json = @"{'big-values':[ { 'big-value':99999999 }]}".Replace("'", "\"");
            var cfg = new Big(json);
            var problems = cfg.Problems();

            foreach (var problem in problems)
            {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void CamelCaseXml() {
            const string xml = @"<cfg><bigValues><add bigValue=""99999999"" /></bigValues></cfg>";
            var cfg = new Big(xml);
            var problems = cfg.Problems();

            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void CamelCaseJson() {
            var json = @"{ 'bigValues': [ { 'bigValue':99999999 }]}".Replace("'", "\"");
            var cfg = new Big(json);

            var problems = cfg.Problems();
            foreach (var problem in problems)
            {
                Console.WriteLine(problem);
            }

            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void TitleCaseXml() {
            const string xml = @"<cfg><BigValues><add BigValue=""99999999"" /></BigValues></cfg>";
            var cfg = new Big(xml);
            var problems = cfg.Problems();

            Assert.AreEqual(0, problems.Count);
            Assert.AreEqual(99999999, cfg.BigValues.First().BigValue);
        }

        public void TitleCaseJson() {
            var json = @"{ 'BigValues': [ { 'BigValue':99999999 }]}".Replace("'", "\"");
            var cfg = new Big(json);
            var problems = cfg.Problems();

            Assert.AreEqual(0, problems.Count);
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
