using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Parsers {

    public class CollectionNode : INode {
        public CollectionNode(string name) {
            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();
        }
        public string Name { get; }
        public List<IAttribute> Attributes { get; }
        public List<INode> SubNodes { get; }
        public bool TryAttribute(string name, out IAttribute attr) {
            attr = null;
            return false;
        }
    }
}