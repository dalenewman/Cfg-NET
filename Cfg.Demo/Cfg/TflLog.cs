using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflLog : CfgNode {
        public TflLog() {

            Property(name: "name", value: string.Empty, required:true, unique:true);
            Property(name: "provider", value: "[default]");
            Property(name: "layout", value: "[default]");
            Property(name: "level", value: "Informational");
            Property(name: "connection", value: "[default]");
            Property(name: "from", value: "[default]");
            Property(name: "to", value: "[default]");
            Property(name: "subject", value: "[default]");
            Property(name: "file", value: "[default]");
            Property(name: "folder", value: "[default]");
            Property(name: "async", value: false);
        }

    }
}