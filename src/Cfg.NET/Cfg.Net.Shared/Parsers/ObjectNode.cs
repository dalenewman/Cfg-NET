// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2018 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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