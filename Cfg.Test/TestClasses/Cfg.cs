using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses {

    public class Cfg : CfgNode {
        [Cfg(required = true)]
        public List<CfgServer> Servers { get; set; }

        public Cfg(string cfg) {
            this.Load(cfg);
        }

    }
}