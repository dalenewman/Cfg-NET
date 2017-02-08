using System.Collections.Generic;
using System.Runtime.Remoting;
using Cfg.Net;
using NUnit.Framework;

namespace Cfg.Test {

    [TestFixture]
    public class ConfigureWithCode {

        [Test]
        public void Test1() {

            var example = new Example();
            Assert.AreEqual(0, example.Errors().Length);
            Assert.IsNullOrEmpty(example.Name);
            Assert.IsNotNull(example.Items);

            example.Check();
            Assert.AreNotEqual(0, example.Errors().Length);
            Assert.AreEqual(1, example.Warnings().Length);
            Assert.AreEqual(2, example.Errors().Length);

            example.Items.Add(new ExampleItem { Value = 5});
            example.Items.Add(new ExampleItem { Value = 6});
            example.Check();
            Assert.AreNotEqual(0, example.Errors().Length);
            Assert.AreEqual(1, example.Warnings().Length);
            Assert.AreEqual(1, example.Errors().Length, "Only 1 error because we fixed the missing items.");

            example.Name = "I am required";
            example.Check();
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
