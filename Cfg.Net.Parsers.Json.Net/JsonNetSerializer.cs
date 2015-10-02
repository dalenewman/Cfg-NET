using Cfg.Net.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cfg.Net.Parsers.Json.Net {
    public class JsonNetSerializer : ISerializer {
        private readonly JsonSerializerSettings _settings;

        public JsonNetSerializer() {
            _settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        public string Serialize(CfgNode node) {
            return JsonConvert.SerializeObject(node, _settings);
        }
    }
}