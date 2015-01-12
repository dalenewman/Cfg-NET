using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {

    public class TflParameter : CfgNode {

        public TflParameter() {
            Property("entity", string.Empty);
            Property("field", string.Empty);
            Property("name", string.Empty);
            Property("value", string.Empty);
            Property("input", true);
            Property("type", "string");
        }

    }

}