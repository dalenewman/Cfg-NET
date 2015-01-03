using Transformalize.Libs.Cfg;

namespace Cfg.Test.TestClasses {
    public class AttributeSite : CfgNode {

        [Cfg(value = "", required = true, unique = true)]
        public string Name { get; set; }

        [Cfg(value = "", required = true)]
        public string Url { get; set; }

        [Cfg(value = "", decode = true)]
        public string Something { get; set; }

        [Cfg(value = 0)]
        public int Numeric { get; set; }

    }
}