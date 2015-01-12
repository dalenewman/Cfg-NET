using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflResponse : CfgNode {
        public TflResponse() {
            Collection<TflRow>("rows", false);
        }
    }
}