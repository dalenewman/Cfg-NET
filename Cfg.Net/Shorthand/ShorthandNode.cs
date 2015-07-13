using System.Collections.Generic;
using Transformalize.Libs.Cfg.Net.Parsers;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
    internal class ShorthandNode : INode {
        public ShorthandNode(string name) {
            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();
        }

        public string Name { get; private set; }
        public List<IAttribute> Attributes { get; private set; }
        public List<INode> SubNodes { get; private set; }
        public bool TryAttribute(string name, out IAttribute attr) {
            throw new System.NotImplementedException();
        }
    }
}