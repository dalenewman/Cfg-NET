using Cfg.Net.Contracts;
using Cfg.Net.Parsers;
using Newtonsoft.Json.Linq;

namespace Cfg.Test.Parsers
{
    public class JsonNetParser : IParser {
        public INode Parse(string cfg) {
            return new JsonNetNode("root", JObject.Parse(cfg));
        }
    }
}