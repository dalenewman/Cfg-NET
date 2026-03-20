// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2026 Dale Newman
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
using System.Text.Json;
using Cfg.Net.Contracts;
using Cfg.Net.Parsers;

namespace Cfg.Net.Parsers.Json {

    public class JsonNode : INode {

        public string Name { get; }
        public List<IAttribute> Attributes { get; }
        public List<INode> SubNodes { get; }

        public JsonNode(string name, JsonElement element) {

            Name = name;
            Attributes = new List<IAttribute>();
            SubNodes = new List<INode>();

            if (element.ValueKind != JsonValueKind.Object)
                return;

            foreach (var property in element.EnumerateObject()) {
                var key = property.Name;
                var value = property.Value;
                switch (value.ValueKind) {
                    case JsonValueKind.Object:
                        break;
                    case JsonValueKind.Null:
                        break;
                    case JsonValueKind.Undefined:
                        break;
                    case JsonValueKind.Array:
                        var subNode = new JsonNode(key, default);
                        foreach (var item in value.EnumerateArray()) {
                            subNode.SubNodes.Add(new JsonNode("add", item));
                        }
                        SubNodes.Add(subNode);
                        break;
                    default: // String, Number, True, False
                        Attributes.Add(new NodeAttribute() { Name = key, Value = GetValue(value) });
                        break;
                }
            }

        }

        private static object GetValue(JsonElement element) {
            switch (element.ValueKind) {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var longVal))
                        return longVal;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    return element.GetRawText();
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
