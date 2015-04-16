using Transformalize.Libs.Cfg.Net.Parsers.fastJSON;

namespace Transformalize.Libs.Cfg.Net.Parsers {

    public class FastJsonParser : IParser {

        public INode Parse(string cfg) {
            return new JsonNode(JSON.Parse(cfg));
        }
    }
}