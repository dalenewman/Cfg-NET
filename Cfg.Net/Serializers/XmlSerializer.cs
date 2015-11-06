using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cfg.Net.Contracts;

namespace Cfg.Net.Serializers {
    public class XmlSerializer : ISerializer {
        public string Serialize(CfgNode node) {
            return InnerSerialize(node);
        }

        private string InnerSerialize(CfgNode node) {

            var type = node.GetType();
            var meta = CfgMetadataCache.GetMetadata(type);
            var builder = new StringBuilder();

            if (JustAttributes(meta)) {
                builder.Append("<");
                builder.Append(type.Name);
                SerializeAttributes(meta, node, builder);
                builder.AppendLine(">");
                SerializeElements(meta, node, builder, 1);
                builder.Append("</");
                builder.Append(type.Name);
                builder.Append(">");
            } else {
                builder.Append("<add");
                SerializeAttributes(meta, node, builder);
                builder.Append(" />");
            }

            return builder.ToString();
        }

        private static bool JustAttributes(Dictionary<string, CfgMetadata> meta) {
            return meta.Any(kv => kv.Value.ListType != null);
        }

        private void SerializeElements(IDictionary<string, CfgMetadata> meta, object node, StringBuilder builder, int level) {

            foreach (var pair in meta.Where(kv => kv.Value.ListType != null)) {
                var items = (IList)meta[pair.Key].Getter(node);
                if (items == null || items.Count == 0)
                    continue;

                Indent(builder, level);
                builder.Append("<");
                builder.Append(pair.Key);
                builder.AppendLine(">");

                foreach (var item in items) {
                    var metaData = CfgMetadataCache.GetMetadata(item.GetType());
                    Indent(builder, level + 1);
                    builder.Append("<add");
                    SerializeAttributes(metaData, item, builder);
                    if (metaData.Any(kv => kv.Value.ListType != null)) {
                        builder.AppendLine(">");
                        SerializeElements(metaData, item, builder, level + 2);
                        Indent(builder, level + 1);
                        builder.AppendLine("</add>");
                    } else {
                        builder.AppendLine(" />");
                    }
                }

                Indent(builder, level);
                builder.Append("</");
                builder.Append(pair.Key);
                builder.AppendLine(">");
            }

        }

        private static void Indent(StringBuilder builder, int level) {
            for (var i = 0; i < level * 4; i++) {
                builder.Append(' ');
            }
        }

        private void SerializeAttributes(Dictionary<string, CfgMetadata> meta, object obj, StringBuilder builder) {
            if (meta.Count > 0) {
                foreach (var pair in meta.Where(kv => kv.Value.ListType == null)) {
                    var value = pair.Value.Getter(obj);
                    if (value == null || value.Equals(pair.Value.Attribute.value) || (!pair.Value.Attribute.ValueIsSet && pair.Value.Default != null && pair.Value.Default.Equals(value))) {
                        continue;
                    }

                    builder.Append(" ");
                    builder.Append(pair.Key);
                    builder.Append("=\"");

                    var stringValue = pair.Value.PropertyInfo.PropertyType == typeof(string) ? (string)value : value.ToString();
                    if (pair.Value.PropertyInfo.PropertyType == typeof(bool)) {
                        stringValue = stringValue.ToLower();
                    }
                    builder.Append(Encode(stringValue));
                    builder.Append("\"");
                }
            } else if (obj is Dictionary<string, string>) {
                foreach (var pair in (Dictionary<string, string>)obj) {
                    builder.Append(" ");
                    builder.Append(pair.Key);
                    builder.Append("=\"");
                    builder.Append(pair.Value == null ? string.Empty : Encode(pair.Value));
                    builder.Append("\"");
                }
            }
        }

        public string Encode(string value) {
            var builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++) {
                char ch = value[i];
                if (ch <= '>') {
                    switch (ch) {
                        case '<':
                            builder.Append("&lt;");
                            break;
                        case '>':
                            builder.Append("&gt;");
                            break;
                        case '"':
                            builder.Append("&quot;");
                            break;
                        case '\'':
                            builder.Append("&#39;");
                            break;
                        case '&':
                            builder.Append("&amp;");
                            break;
                        default:
                            builder.Append(ch);
                            break;
                    }
                } else {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }


    }
}