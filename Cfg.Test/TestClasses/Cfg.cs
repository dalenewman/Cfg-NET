using System.Collections.Generic;
using Cfg.Net;
using Cfg.Net.Contracts;

namespace Cfg.Test.TestClasses {

    public sealed class Cfg : CfgNode {

        [Cfg(required = true)]
        public List<CfgServer> Servers { get; set; }

        public Cfg(string cfg, IParser parser):base(parser) {
            Load(cfg);
        }

    } 
}