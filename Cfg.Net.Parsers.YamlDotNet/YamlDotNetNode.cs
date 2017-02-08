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

namespace Cfg.Net.Parsers.YamlDotNet {

    public class YamlDotNetNode : INode {

        public string Name { get; }
        public List<IAttribute> Attributes { get; }
        public List<INode> SubNodes { get; }

        public YamlDotNetNode(string name, Dictionary<object, object> node) {

            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();

            if (node == null)
                return;

            foreach (var pair in node) {
                var key = pair.Key.ToString();
                var token = pair.Value;
                var list = token as List<object>;
                if (list != null) {
                    if (list.Count > 0) {
                        var subNode = new YamlDotNetNode(key, null);
                        foreach (var item in list) {
                            subNode.SubNodes.Add(new YamlDotNetNode("add", (Dictionary<object, object>)item));
                        }
                        SubNodes.Add(subNode);
                    }
                    continue;
                }

                Attributes.Add(new NodeAttribute { Name = key, Value = token });
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