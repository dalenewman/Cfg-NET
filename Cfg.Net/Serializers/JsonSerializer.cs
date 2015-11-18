using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cfg.Net.Contracts;
using Cfg.Net.Shorthand;

namespace Cfg.Net.Serializers {
    public class JsonSerializer : ISerializer {
        public string Serialize(CfgNode node) {
            return InnerSerialize(node);
        }

        private string InnerSerialize(CfgNode node) {
            var meta = CfgMetadataCache.GetMetadata(node.GetType());
            var builder = new StringBuilder();
            if (meta.All(kv => kv.Value.ListType == null)) {
                builder.Append("{");
                SerializeAttributes(meta, node, builder);
                builder.Append(" }");
            } else {
                builder.AppendLine("{");
                SerializeAttributes(meta, node, builder);
                SerializeElements(meta, node, builder, 1);
                builder.AppendLine();
                builder.Append("}");
            }

            return builder.ToString();
        }

        private void SerializeElements(IDictionary<string, CfgMetadata> meta, object node, StringBuilder sb, int level) {

            var pairs = meta.Where(kv => kv.Value.ListType != null).ToArray();

            for (var y = 0; y < pairs.Length; y++) {

                var pair = pairs[y];
                var nodes = (IList)meta[pair.Key].Getter(node);
                if (nodes == null || nodes.Count == 0)
                    continue;

                Indent(sb, level);
                sb.Append("\"");
                sb.Append(pair.Key);
                sb.AppendLine("\":[");

                var count = nodes.Count;
                var last = count - 1;
                for (var i = 0; i < count; i++) {
                    var item = nodes[i];
                    var metaData = CfgMetadataCache.GetMetadata(item.GetType());
                    Indent(sb, level + 1);
                    sb.Append("{");
                    SerializeAttributes(metaData, item, sb);
                    if (metaData.Any(kv => kv.Value.ListType != null)) {
                        SerializeElements(metaData, item, sb, level + 2);
                        Indent(sb, level + 1);
                        Next(sb, i, last);
                    } else {
                        Next(sb, i, last);
                    }
                }

                Indent(sb, level);
                sb.Append("]");

                if (y < pairs.Length - 1) {
                    sb.Append(",");
                }

            }

        }

        private static void Next(StringBuilder sb, int i, int last) {
            if (i < last) {
                sb.Append(" }");
                sb.AppendLine(",");
            } else {
                sb.AppendLine(" }");
            }
        }

        private static void Indent(StringBuilder builder, int level) {
            for (var i = 0; i < level * 4; i++) {
                builder.Append(' ');
            }
        }

        private void SerializeAttributes(Dictionary<string, CfgMetadata> meta, object obj, StringBuilder sb) {

            int last;
            if (meta.Count > 0) {
                var pairs = meta.Where(kv => kv.Value.ListType == null).ToArray();
                last = pairs.Length - 1;
                for (int i = 0; i < pairs.Length; i++) {
                    var pair = pairs[i];
                    var value = pair.Value.Getter(obj);
                    if (value == null || value.Equals(pair.Value.Attribute.value) || (!pair.Value.Attribute.ValueIsSet && pair.Value.Default != null && pair.Value.Default.Equals(value))) {
                        if (i == last) {
                            sb.Remove(sb.Length-1,1);
                        }
                        continue;
                    }

                    string stringValue;
                    var type = pair.Value.PropertyInfo.PropertyType;

                    if (type == typeof(string)) {
                        stringValue = "\"" + Encode((string)value) + "\"";
                    } else if (type == typeof(bool)) {
                        stringValue = value.ToString().ToLower();
                    } else if (type == typeof(DateTime)) {
                        stringValue = "\"" + ((DateTime)value).ToString("o") + "\"";
                    } else if (type == typeof(Guid)) {
                        stringValue = "\"" + ((Guid)value) + "\"";
                    } else {
                        stringValue = value.ToString();
                    }

                    sb.Append(" \"");
                    sb.Append(pair.Key);
                    sb.Append("\":");
                    sb.Append(stringValue);
                    if (i < last) {
                        sb.Append(",");
                    }

                }

            } else if (obj is Dictionary<string, string>) {
                var dict = (Dictionary<string, string>)obj;
                var count = 0;
                last = dict.Count - 1;
                foreach (var pair in dict) {
                    sb.Append(" \"");
                    sb.Append(pair.Key);
                    sb.Append("\":");
                    sb.Append(Encode(pair.Value));
                    if (count < last) {
                        sb.Append(",");
                    }
                    count++;
                }
            }

        }

        public string Encode(string value) {
            if (value.Length == 0) {
                return string.Empty;
            }

            int i;
            var len = value.Length;
            var sb = new StringBuilder(len + 4);

            for (i = 0; i < len; i += 1) {
                var c = value[i];
                switch (c) {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ') {
                            var t = "000" + BytesToHexString(new[] { Convert.ToByte(c) });
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        } else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        private static string BytesToHexString(byte[] bytes) {
            var c = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++) {
                var b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }
    }
}