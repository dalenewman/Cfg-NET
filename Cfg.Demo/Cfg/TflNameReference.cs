using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflNameReference : CfgNode {
        public TflNameReference() {
            Property(name: "name", value: string.Empty, required: true, unique: true);
        }

    }
}