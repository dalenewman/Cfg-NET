using Cfg.Net.Contracts;
using Newtonsoft.Json.Linq;

namespace Cfg.Net.Parsers.Json.Net {
    public class JsonNetParser : IParser {
        public INode Parse(string cfg) {
            return new JsonNetNode("root", JObject.Parse(cfg));
        }

        public string Decode(string value) {
            return value;
        }
    }
}