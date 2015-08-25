using Cfg.Net;

namespace Cfg.Test.TestClasses {
    public class CfgParameter : CfgNode {

        [Cfg(required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public string Value { get; set; }
    }
}