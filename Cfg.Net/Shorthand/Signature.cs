using System.Collections.Generic;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Signature : CfgNode {

        [Cfg(required = true, unique = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(required = false)]
        public List<Parameter> Parameters { get; set; }
    }
}