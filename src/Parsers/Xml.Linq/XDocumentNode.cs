// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2022 Dale Newman
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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.Parsers.Xml.Linq {

    public class XDocumentNode : INode {

        public string Name { get; }
        public List<IAttribute> Attributes { get; }
        public List<INode> SubNodes { get; }

        public XDocumentNode(XElement node) {
            Name = node.Name.LocalName;
            Attributes = new List<IAttribute>(node.Attributes().Select(a => new NodeAttribute() { Name = a.Name.LocalName, Value = a.Value }));
            SubNodes = new List<INode>(node.Elements().Select(n => new XDocumentNode(n)));
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