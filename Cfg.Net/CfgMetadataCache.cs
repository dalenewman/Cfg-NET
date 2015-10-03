using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cfg.Net {
    internal static class CfgMetadataCache {

        private static readonly object Locker = new object();
        private static readonly Dictionary<Type, Dictionary<string, CfgMetadata>> MetadataCache = new Dictionary<Type, Dictionary<string, CfgMetadata>>();
        private static readonly Dictionary<Type, List<string>> PropertyCache = new Dictionary<Type, List<string>>();
        private static readonly Dictionary<Type, List<string>> ElementCache = new Dictionary<Type, List<string>>();
        private static readonly Dictionary<Type, Dictionary<string, string>> NameCache = new Dictionary<Type, Dictionary<string, string>>();

        internal static Dictionary<string, CfgMetadata> GetMetadata(Type type, CfgEvents events = null) {
            Dictionary<string, CfgMetadata> metadata;
            if (MetadataCache.TryGetValue(type, out metadata))
                return metadata;

            lock (Locker) {
                NameCache[type] = new Dictionary<string, string>();

                var keyCache = new List<string>();
                var listCache = new List<string>();
                PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                metadata = new Dictionary<string, CfgMetadata>(StringComparer.Ordinal);
                for (int i = 0; i < propertyInfos.Length; i++) {
                    PropertyInfo propertyInfo = propertyInfos[i];

                    if (!propertyInfo.CanRead)
                        continue;
                    if (!propertyInfo.CanWrite)
                        continue;
                    var attribute = (CfgAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CfgAttribute), true);
                    if (attribute == null)
                        continue;

                    var key = NormalizeName(type, propertyInfo.Name);
                    var item = new CfgMetadata(propertyInfo, attribute);

                    // check default value for type mismatch
                    if (attribute.ValueIsSet) {
                        if (attribute.value.GetType() != propertyInfo.PropertyType) {
                            object value = attribute.value;
                            if (TryConvertValue(ref value, propertyInfo.PropertyType)) {
                                attribute.value = value;
                            } else {
                                item.TypeMismatch = true;
                                events?.TypeMismatch(key, value, propertyInfo.PropertyType);
                            }
                        }
                    }

                    // type safety for value, min value, and max value
                    var defaultValue = attribute.value;
                    if (ResolveType(() => attribute.ValueIsSet, ref defaultValue, key, item, events)) {
                        attribute.value = defaultValue;
                    }

                    var minValue = attribute.minValue;
                    if (ResolveType(() => attribute.MinValueSet, ref minValue, key, item, events)) {
                        attribute.minValue = minValue;
                    }

                    var maxValue = attribute.maxValue;
                    if (ResolveType(() => attribute.MaxValueSet, ref maxValue, key, item, events)) {
                        attribute.maxValue = maxValue;
                    }

                    if (propertyInfo.PropertyType.IsGenericType) {
                        listCache.Add(key);
                        item.ListType = propertyInfo.PropertyType.GetGenericArguments()[0];
                        if (item.ListType.IsSubclassOf(typeof(CfgNode))) {
                            item.Loader = () => (CfgNode)Activator.CreateInstance(item.ListType);
                        }
                    } else {
                        keyCache.Add(key);
                    }
                    item.Setter = CfgReflectionHelper.CreateSetter(propertyInfo);
                    item.Getter = CfgReflectionHelper.CreateGetter(propertyInfo);

                    metadata[key] = item;
                }

                PropertyCache[type] = keyCache;
                ElementCache[type] = listCache;
                MetadataCache[type] = metadata;

                return metadata;
            }

        }

        private static bool ResolveType(Func<bool> isSet, ref object input, string key, CfgMetadata metadata, CfgEvents events) {
            if (!isSet())
                return true;

            var type = metadata.PropertyInfo.PropertyType;

            if (input.GetType() == type)
                return true;

            var value = input;
            if (TryConvertValue(ref value, type)) {
                input = value;
                return true;
            }

            metadata.TypeMismatch = true;
            events?.TypeMismatch(key, value, type);
            return false;
        }

        private static bool TryConvertValue(ref object value, Type conversionType) {
            try {
                value = Convert.ChangeType(value, conversionType, null);
                return true;
            } catch {
                return false;
            }
        }

        internal static string NormalizeName(Type type, string name) {
            lock (Locker) {
                string value;
                if (NameCache[type].TryGetValue(name, out value)) {
                    return value;
                }

                var builder = new StringBuilder();
                foreach (var character in name.ToCharArray().Where(char.IsLetterOrDigit)) {
                    builder.Append(char.IsUpper(character) ? char.ToLowerInvariant(character) : character);
                }
                var result = builder.ToString();
                NameCache[type][name] = result;
                return result;
            }
        }


        public static IEnumerable<string> PropertyNames(Type type) {
            List<string> names;
            return PropertyCache.TryGetValue(type, out names) ? names : new List<string>();
        }

        public static IEnumerable<string> ElementNames(Type type) {
            List<string> names;
            return ElementCache.TryGetValue(type, out names) ? names : new List<string>();
        }

        /// <summary>
        /// Clone any node that has a parameterless constructor defined.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static object Clone(CfgNode node) {
            var type = node.GetType();
            var meta = GetMetadata(type);
            var clone = Activator.CreateInstance(type);
            CloneProperties(meta, node, clone);
            CloneLists(meta, node, clone);
            return clone;
        }

        private static void CloneProperties(Dictionary<string, CfgMetadata> meta, object node, object clone) {
            foreach (var pair in meta.Where(kv => kv.Value.ListType == null)) {
                pair.Value.Setter(clone, pair.Value.Getter(node));
            }
        }

        private static void CloneLists(IDictionary<string, CfgMetadata> meta, object node, object clone) {
            foreach (var pair in meta.Where(kv => kv.Value.ListType != null)) {
                var items = (IList)meta[pair.Key].Getter(node);
                var cloneItems = (IList)Activator.CreateInstance(pair.Value.PropertyInfo.PropertyType);
                foreach (var item in items) {
                    var metaItem = GetMetadata(item.GetType());
                    var cloneItem = Activator.CreateInstance(pair.Value.ListType);
                    CloneProperties(metaItem, item, cloneItem);
                    CloneLists(metaItem, item, cloneItem);
                    cloneItems.Add(cloneItem);
                }
                meta[pair.Key].Setter(clone, cloneItems);
            }
        }
    }
}