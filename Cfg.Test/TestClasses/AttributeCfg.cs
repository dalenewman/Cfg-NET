using System.Collections.Generic;
using Cfg.Net;

namespace Cfg.Test.TestClasses {

    public class AttributeCfg : CfgNode {

        public AttributeCfg(string xml) {
            this.Load(xml);
        }

        [Cfg(required = true, sharedProperty = "common", sharedValue = "")]
        public List<AttributeSite> Sites { get; set; }
    }

}