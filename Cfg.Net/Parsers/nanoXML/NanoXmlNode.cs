using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transformalize.Libs.Cfg.Net.Parsers.nanoXML
{
    /// <summary>
    ///     Element node of document
    /// </summary>
    public class NanoXmlNode : NanoXmlBase {
        private readonly List<NanoXmlAttribute> _attributes = new List<NanoXmlAttribute>();
        private readonly string _name;

        private readonly List<NanoXmlNode> _subNodes = new List<NanoXmlNode>();

        internal NanoXmlNode(string str, ref int i) {
            _name = ParseAttributes(str, ref i, _attributes, '>', '/');

            if (str[i] == '/') // if this node has nothing inside
            {
                i++; // skip /
                i++; // skip >
                return;
            }

            i++; // skip >

            // temporary. to include all whitespaces into value, if any
            var tempI = i;

            SkipSpaces(str, ref tempI);

            if (str[tempI] == '<') {
                i = tempI;

                while (str[i + 1] != '/') // parse subnodes
                {
                    i++; // skip <
                    _subNodes.Add(new NanoXmlNode(str, ref i));

                    SkipSpaces(str, ref i);

                    if (i >= str.Length)
                        return; // EOF

                    if (str[i] != '<')
                        throw new NanoXmlParsingException("Unexpected token");
                }

                i++; // skip <
            } else // parse value
            {
                Value = GetValue(str, ref i, '<', '\0', false);
                i++; // skip <

                if (str[i] != '/')
                    throw new NanoXmlParsingException("Invalid ending on tag " + _name);
            }

            i++; // skip /
            SkipSpaces(str, ref i);

            var endName = GetValue(str, ref i, '>', '\0', true);
            if (endName != _name)
                throw new NanoXmlParsingException("Start/end tag name mismatch: " + _name + " and " + endName);
            SkipSpaces(str, ref i);

            if (str[i] != '>')
                throw new NanoXmlParsingException("Invalid ending on tag " + _name);

            i++; // skip >
        }

        /// <summary>
        ///     Element value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Element name
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        ///     List of subelements
        /// </summary>
        public List<NanoXmlNode> SubNodes {
            get { return _subNodes; }
        }

        /// <summary>
        ///     List of attributes
        /// </summary>
        public List<NanoXmlAttribute> Attributes {
            get { return _attributes; }
        }

        /// <summary>
        ///     Returns subelement by given name
        /// </summary>
        /// <param name="nodeName">Name of subelement to get</param>
        /// <returns>First subelement with given name or NULL if no such element</returns>
        public NanoXmlNode this[string nodeName] {
            get {
                for (var i = 0; i < _subNodes.Count; i++) {
                    var nanoXmlNode = _subNodes[i];
                    if (nanoXmlNode._name == nodeName)
                        return nanoXmlNode;
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns attribute by given name
        /// </summary>
        /// <param name="attributeName">Attribute name to get</param>
        /// <returns><see cref="NanoXmlAttribute" /> with given name or null if no such attribute</returns>
        public NanoXmlAttribute GetAttribute(string attributeName) {
            for (var i = 0; i < _attributes.Count; i++) {
                var nanoXmlAttribute = _attributes[i];
                if (nanoXmlAttribute.Name == attributeName)
                    return nanoXmlAttribute;
            }

            return null;
        }

        public bool TryAttribute(string attributeName, out NanoXmlAttribute attribute) {
            for (var i = 0; i < _attributes.Count; i++) {
                var nanoXmlAttribute = _attributes[i];
                if (nanoXmlAttribute.Name != attributeName)
                    continue;
                attribute = nanoXmlAttribute;
                return true;
            }

            attribute = null;
            return false;
        }

        public bool HasAttribute(string attributeName) {
            return _attributes.Any(nanoXmlAttribute => nanoXmlAttribute.Name == attributeName);
        }

        public string InnerText() {
            var builder = new StringBuilder();
            InnerText(ref builder);
            return builder.ToString();
        }

        private void InnerText(ref StringBuilder builder) {
            for (var i = 0; i < _subNodes.Count; i++) {
                var node = _subNodes[i];
                builder.Append("<");
                builder.Append(node.Name);
                foreach (var attribute in node._attributes) {
                    builder.AppendFormat(" {0}=\"{1}\"", attribute.Name, attribute.Value);
                }
                builder.Append(">");
                if (node.Value == null) {
                    node.InnerText(ref builder);
                } else {
                    builder.Append(node.Value);
                }
                builder.AppendFormat("</{0}>", node.Name);
            }
        }

        public override string ToString() {
            var builder = new StringBuilder("<");
            builder.Append(Name);
            foreach (var attribute in Attributes) {
                builder.AppendFormat(" {0}=\"{1}\"", attribute.Name, attribute.Value);
            }
            builder.Append(">");
            if (Value == null) {
                InnerText(ref builder);
            } else {
                builder.Append(Value);
            }

            builder.AppendFormat("</{0}>", Name);
            return builder.ToString();
        }

        public bool HasSubNode() {
            return SubNodes != null && SubNodes.Any();
        }
    }
}