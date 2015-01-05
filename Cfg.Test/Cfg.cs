using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test {
    public class Cfg : CfgNode {
        [Cfg(required = true)]
        public List<CfgServer> Servers { get; set; }

        public Cfg(string xml) {
            this.Load(xml);
        }
    }
}