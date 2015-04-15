using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Transformalize.Libs.Cfg.Net {

    public class JsonNode : INode {

        private Dictionary<string, IAttribute> _attributes;

        public JsonNode() {
            Name = string.Empty;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();
        }

        public JsonNode(string name, Dictionary<string, object> parsed) {
            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();
            HandleDictionary(parsed);
        }

        public JsonNode(string name, IList<object> parsed) {
            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();
            HandleList(parsed);
        }

        public JsonNode(object parsed) {

            Name = string.Empty;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();

            //dict
            var dict = parsed as Dictionary<string, object>;
            if (dict != null) {
                HandleDictionary(dict);
                return;
            }

            //list
            var list = parsed as List<object>;
            if (list != null) {
                HandleList(list);
            }
        }

        private void HandleDictionary(Dictionary<string, object> dict) {
            foreach (var pair in dict) {
                ProcessNameAndValue(pair.Key, pair.Value);
            }
        }

        private void ProcessNameAndValue(string name, object value) {
            // objects
            var dict = value as Dictionary<string, object>;
            if (dict != null) {
                SubNodes.Add(new JsonNode(name, dict));
                return;
            }

            // arrays of objects
            var list = value as List<object>;
            if (list != null) {
                SubNodes.Add(new JsonNode(name, list));
                return;
            }

            // shared attributes
            if (name.Contains(".")) {
                var split = name.Split(new[] { '.' });
                var element = split[0];
                var attribute = split[1];
                for (int i = 0; i < SubNodes.Count; i++) {
                    var subNode = SubNodes[i];
                    if (subNode.Name == element) {
                        AddAttribute(subNode.Attributes, attribute, value);
                    }
                }
                return;
            }

            // attributes
            AddAttribute(Attributes, name, value);
        }

        private static void AddAttribute(ICollection<IAttribute> attributes, string name, object value) {

            var str = value as string;
            if (str != null) {
                attributes.Add(new NodeAttribute() { Name = name, Value = str });
                return;
            }

            if (value != null) {
                attributes.Add(new NodeAttribute {
                    Name = name,
                    Value = value.ToString()
                });
            }
        }

        private void HandleList(IList<object> parsed) {
            if (parsed.Count == 0)
                return;

            var first = parsed[0];
            if (first is string || first is long) {
                for (var i = 0; i < parsed.Count; i++) {
                    var dict = new Dictionary<string, object>();
                    dict[i.ToString(CultureInfo.InvariantCulture)] = parsed[i];
                    SubNodes.Add(new JsonNode("add", dict));
                }
            } else {
                for (var i = 0; i < parsed.Count; i++) {
                    ProcessNameAndValue("add", parsed[i]);
                }
            }
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