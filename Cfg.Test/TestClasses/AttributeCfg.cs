using System.Collections.Generic;
using Transformalize.Libs.Cfg;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses {

    public class AttributeCfg : CfgNode {
        [Cfg(required = true, sharedProperty = "common", sharedValue = "x")]
        public List<AttributeSite> Sites { get; set; }
    }

}