using System.Collections.Generic;
using Transformalize.Libs.Cfg;

namespace Cfg.Test.TestClasses {

    public class AttributeCfg : CfgNode {
        [Cfg(required = true)]
        public List<AttributeSite> Sites { get; set; }
    }

}