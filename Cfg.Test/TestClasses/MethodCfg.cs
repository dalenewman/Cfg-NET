using Transformalize.Libs.Cfg.Net;

namespace Cfg.Test.TestClasses {
    public class MethodCfg : CfgNode {
        public MethodCfg() {
            Collection<MethodSite, string>("sites", required: true, sharedProperty: "common", sharedValue: "x");
        }
    }
}