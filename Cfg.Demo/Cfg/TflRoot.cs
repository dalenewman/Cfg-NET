using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {
    public class TflRoot : CfgNode {
        public TflRoot(string xml, Dictionary<string, string> parameters) {
            Collection<TflEnvironment, string>("environments", false, "default", string.Empty);
            Collection<TflProcess>("processes", required:true);
            Collection<TflResponse>("response");
            SharedProperty("response", "status", 0);
            SharedProperty("response", "message", string.Empty);
            SharedProperty("response", "time", 0);
            Load(xml, parameters);
        }
    }
}