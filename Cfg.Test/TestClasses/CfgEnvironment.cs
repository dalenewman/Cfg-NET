using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses
{
    public class CfgEnvironment : CfgNode {

        [Cfg(required = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public List<CfgParameter> Parameters { get; set; }

        //shared property, defined in parent
        public string Default { get; set; }
    }
}