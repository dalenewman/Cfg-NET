using Transformalize.Libs.Cfg;

namespace Cfg.Test.TestClasses {
    public class MethodCfg : CfgNode {
        public MethodCfg() {
            Class<MethodSite>("sites", required: true);
        }
    }
}