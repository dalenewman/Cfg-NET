using System.Collections.Generic;
using System.IO;
using Cfg.Net.Contracts;

namespace Cfg.Net.Parsers.YamlDotNet {
    public class YamlDotNetParser : IParser {
        public INode Parse(string cfg) {
            var deserializer = new global::YamlDotNet.Serialization.Deserializer();
            var dict = deserializer.Deserialize(new StringReader(cfg)) as Dictionary<object,object>;
            return new YamlDotNetNode("cfg", dict);
        }
    }
}