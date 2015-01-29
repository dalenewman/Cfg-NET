using System;
using System.Collections.Generic;
using NUnit.Framework;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {

    [TestFixture]
    public class EmptyLists {

        [Test]
        public void TestConnections() {
            var xml = @"
                <cfg>
                </cfg>
            ".Replace("'", "\"");

            var cfg = new EmptyListCfg(xml);
            var problems = cfg.Problems();

            Assert.AreEqual(0, problems.Count);
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(0, cfg.Connections.Count);

        }

        [Test]
        public void TestOtherThings() {
            var xml = @"
                <cfg>
                    <connections>
                        <add provider='file' file='c:\temp.txt' />
                    </connections>
                </cfg>
            ".Replace("'", "\"");

            var cfg = new EmptyListCfg(xml);
            var problems = cfg.Problems();

            Assert.AreEqual(0, problems.Count);
            Assert.IsNotNull(cfg.Connections);
            Assert.AreEqual(1, cfg.Connections.Count);
            Assert.IsNotNull(cfg.Connections[0].OtherThings);
            Assert.AreEqual(0, cfg.Connections[0].OtherThings.Count);

        }

    }

    public sealed class EmptyListCfg : CfgNode {
        public EmptyListCfg(string xml) {
            Load(xml);
        }

        [Cfg(required = false)]
        public List<EmptyListConnection> Connections { get; set; }

    }

    public class EmptyListConnection : CfgNode {

        [Cfg(required = true, domain = "file,folder,other")]
        public string Provider { get; set; }

        [Cfg()]
        public string File { get; set; }

        [Cfg()]
        public string Folder { get; set; }

        [Cfg()]
        public List<EmptyListOtherThing> OtherThings { get; set; }

    }

    public class EmptyListOtherThing : CfgNode {
        [Cfg()]
        public string Name { get; set; }
    }
}
