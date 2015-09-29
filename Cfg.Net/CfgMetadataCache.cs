using System;
using System.Collections.Generic;
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
                    object defaultValue = attribute.value;
                    if (ResolveType(() => attribute.ValueIsSet, ref defaultValue, key, item, events)) {
                        attribute.value = defaultValue;
                    }

                    object minValue = attribute.minValue;
                    if (ResolveType(() => attribute.MinValueSet, ref minValue, key, item, events)) {
                        attribute.minValue = minValue;
                    }

                    object maxValue = attribute.maxValue;
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
            }

            return MetadataCache[type];
        }

        private static bool ResolveType(Func<bool> isSet, ref object input, string key, CfgMetadata metadata, CfgEvents events) {
            if (!isSet())
                return true;

            var type = metadata.PropertyInfo.PropertyType;

            if (input.GetType() == type)
                return true;

            object value = input;
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

        public static string NormalizeName(Type type, string name) {
            var cache = NameCache[type];
            if (cache.ContainsKey(name)) {
                return cache[name];
            }
            var builder = new StringBuilder();
            for (var i = 0; i < name.Length; i++) {
                var character = name[i];
                if (char.IsLetterOrDigit(character)) {
                    builder.Append(char.IsUpper(character) ? char.ToLowerInvariant(character) : character);
                }
            }
            string result = builder.ToString();
            cache[name] = result;
            return result;
        }


        public static IEnumerable<string> PropertyNames(Type type) {
            return PropertyCache.ContainsKey(type) ? PropertyCache[type] : new List<string>();
        }

        public static IEnumerable<string> ElementNames(Type type) {
            return ElementCache.ContainsKey(type) ? ElementCache[type] : new List<string>();
        }
    }
}