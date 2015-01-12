using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflEnvironment : CfgNode {
        public TflEnvironment() {
            Property("name", string.Empty, true, true);
            Collection<TflParameter>("parameters", true);
        }
    }
}