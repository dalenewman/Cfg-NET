using Cfg.Net.Contracts;

namespace Cfg.Net.Serializers {
    public class JsonSerializer : ISerializer {
        public string Serialize(CfgNode node) {
            throw new System.NotImplementedException();
        }

        public string Encode(string value) {
            return value;
        }
    }
}