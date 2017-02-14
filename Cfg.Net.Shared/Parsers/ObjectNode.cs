using System.Collections;
using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Parsers {
    internal sealed class ObjectNode : INode {

        private readonly Dictionary<string, IAttribute> _attributes = new Dictionary<string, IAttribute>();

        public ObjectNode(object cfgNode, string name) {

            var type = cfgNode.GetType();
            var metadata = CfgMetadataCache.GetMetadata(type);
            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();

            foreach (var pair in metadata) {
                if (pair.Value.ListType == null) {
                    // get the value, create attribute
                    var value = pair.Value.Getter(cfgNode);
                    var attribute = new NodeAttribute(pair.Value.Attribute.name, value);
                    _attributes[attribute.Name] = attribute;
                    Attributes.Add(attribute);
                } else {
                    // get the list, transfer it to collection node of sub nodes
                    var list = (IList)pair.Value.Getter(cfgNode);
                    if (list == null)
                        continue;
                    var collection = new CollectionNode(pair.Value.Attribute.name);
                    foreach (var item in list) {
                        collection.SubNodes.Add(new ObjectNode(item, "add"));
                    }
                    SubNodes.Add(collection);
                }
            }
        }

        public string Name { get; }
        public List<IAttribute> Attributes { get; }
        public List<INode> SubNodes { get; }
        public bool TryAttribute(string name, out IAttribute attr) {
            if (_attributes.ContainsKey(name)) {
                attr = _attributes[name];
                return true;
            }
            attr = null;
            return false;
        }
    }
}