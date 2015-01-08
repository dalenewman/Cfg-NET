using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses {
    public class CfgServer : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public List<CfgDatabase> Databases { get; set; }

    }
}