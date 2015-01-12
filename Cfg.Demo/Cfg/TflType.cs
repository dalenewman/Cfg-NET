using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflType : CfgNode {
        public TflType() {
            Property(name:"type", value:string.Empty, required:true, unique:true);
        }
    }
}