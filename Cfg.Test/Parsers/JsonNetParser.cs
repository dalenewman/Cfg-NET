using Newtonsoft.Json.Linq;
using Transformalize.Libs.Cfg.Net.Parsers;

namespace Cfg.Test.Parsers
{
    public class JsonNetParser : IParser {
        public INode Parse(string cfg) {
            return new JsonNetNode("root", JObject.Parse(cfg));
        }
    }
}