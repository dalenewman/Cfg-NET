using System.Collections.Generic;
using System.Linq;
using Transformalize.Libs.Cfg.Net.Parsers.nanoXML;

namespace Transformalize.Libs.Cfg.Net.Parsers {

    public sealed class XmlNode : INode {

        private Dictionary<string, IAttribute> _attributes;

        public XmlNode() { }

        public XmlNode(NanoXmlNode nanoXmlNode) {
            Name = nanoXmlNode.Name;
            Attributes = new List<IAttribute>(nanoXmlNode.Attributes.Select(a => new NodeAttribute() { Name = a.Name, Value = a.Value }));
            SubNodes = new List<INode>(nanoXmlNode.SubNodes.Select(n => new XmlNode(n)));
        }

        public string Name { get; private set; }
        public List<IAttribute> Attributes { get; private set; }
        public List<INode> SubNodes { get; private set; }

        public bool TryAttribute(string name, out IAttribute attr) {
            if (_attributes == null) {
                _attributes = new Dictionary<string, IAttribute>();
                for (var i = 0; i < Attributes.Count; i++) {
                    _attributes[Attributes[i].Name] = Attributes[i];
                }
            }
            if (_attributes.ContainsKey(name)) {
                attr = _attributes[name];
                return true;
            }
            attr = null;
            return false;
        }

    }
}