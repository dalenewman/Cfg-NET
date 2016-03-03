using System;
using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.MergeParameters {
    public class MergeParameters : IMergeParameters {
        private readonly string _nameAttribute;
        private readonly string _valueAttribute;

        public MergeParameters() : this("name", "value") {}

        public MergeParameters(string nameAttribute, string valueAttribute) {
            _nameAttribute = nameAttribute;
            _valueAttribute = valueAttribute;
        }

        public IDictionary<string, string> Merge(INode root, IDictionary<string, string> parameters) {
            parameters = parameters ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var j = 0; j < root.SubNodes.Count; j++) {
                var parameter = root.SubNodes[j];
                string name = null;
                string value = null;
                for (var k = 0; k < parameter.Attributes.Count; k++) {
                    var attribute = parameter.Attributes[k];
                    if (attribute.Name == _nameAttribute) {
                        name = attribute.Value;
                    } else if (attribute.Name == _valueAttribute) {
                        value = attribute.Value;
                    }
                }
                if (name != null && value != null && !parameters.ContainsKey(name)) {
                    parameters[name] = value;
                }
            }
            return parameters;
        }
    }
}