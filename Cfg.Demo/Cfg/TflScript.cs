using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflScript : CfgNode {

        public TflScript() {
            Property(name: "name", value: string.Empty, required: true, unique: true);
            Property(name: "file", value: string.Empty, required: true);
        }

    }
}