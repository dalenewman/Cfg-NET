using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses {

    public class AttributeCfg : CfgNode {

        public AttributeCfg(string xml) {
            this.Load(xml);
        }

        [Cfg(required = true, sharedProperty = "common", sharedValue = "x")]
        public List<AttributeSite> Sites { get; set; }
    }

}