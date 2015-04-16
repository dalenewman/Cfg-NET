using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;
using Transformalize.Libs.Cfg.Net.Parsers;

namespace Cfg.Test.TestClasses {

    public sealed class Cfg : CfgNode {

        [Cfg(required = true)]
        public List<CfgServer> Servers { get; set; }

        public Cfg(string cfg, IParser parser):base(parser) {
            this.Load(cfg);
        }

    }
}