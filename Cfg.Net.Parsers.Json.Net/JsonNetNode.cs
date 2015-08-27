using System.Collections.Generic;
using System.Linq;
using Cfg.Net.Contracts;
using Newtonsoft.Json.Linq;

namespace Cfg.Net.Parsers.Json.Net {

    public class JsonNetNode : INode {

        public string Name { get; }
        public List<IAttribute> Attributes { get; }
        public List<INode> SubNodes { get; }

        public JsonNetNode(string name, JObject node) {

            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();

            if (node == null)
                return;

            foreach (var pair in node) {
                var key = pair.Key;
                var token = pair.Value;
                switch (token.Type) {
                    case JTokenType.Object:
                        break;
                    case JTokenType.Property:
                        break;
                    case JTokenType.None:
                        break;
                    case JTokenType.Null:
                        break;
                    case JTokenType.Array:
                        var subNode = new JsonNetNode(key, null);
                        foreach (var item in (JArray)token) {
                            subNode.SubNodes.Add(new JsonNetNode("add", (JObject)item));
                        }
                        SubNodes.Add(subNode);
                        break;
                    default:
                        Attributes.Add(new NodeAttribute() { Name = key, Value = token.ToString() });
                        break;
                }
            }

        }

        public bool TryAttribute(string name, out IAttribute attr) {
            if (Attributes.Any()) {
                if (Attributes.Exists(a => a.Name == name)) {
                    attr = Attributes.First(a => a.Name == name);
                    return true;
                }
            }
            attr = null;
            return false;
        }
    }
}