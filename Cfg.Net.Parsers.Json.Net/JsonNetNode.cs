#region license
// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2017 Dale Newman
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
#endregion
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
                    default:  // a value
                        Attributes.Add(new NodeAttribute() { Name = key, Value = token });
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