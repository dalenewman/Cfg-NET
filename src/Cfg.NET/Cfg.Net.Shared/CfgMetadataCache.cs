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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Cfg.Net.Contracts;

namespace Cfg.Net {
    internal static class CfgMetadataCache {

        private static readonly object Locker = new object();
        private static readonly Dictionary<Type, Dictionary<string, CfgMetadata>> MetadataCache = new Dictionary<Type, Dictionary<string, CfgMetadata>>();
        private static readonly Dictionary<Type, List<string>> PropertyCache = new Dictionary<Type, List<string>>();
        private static readonly Dictionary<Type, List<string>> ElementCache = new Dictionary<Type, List<string>>();
        private static readonly Dictionary<Type, Dictionary<string, string>> NameCache = new Dictionary<Type, Dictionary<string, string>>();

        internal static Dictionary<string, CfgMetadata> GetMetadata(Type type) {
            if (MetadataCache.TryGetValue(type, out var metadata))
                return metadata;

            lock (Locker) {
                NameCache[type] = new Dictionary<string, string>();

                var keyCache = new List<string>();
                var listCache = new List<string>();

#if NETS
                var propertyInfos = type.GetRuntimeProperties().ToArray();
#else
                var propertyInfos = type.GetProperties();
#endif

                metadata = new Dictionary<string, CfgMetadata>(StringComparer.Ordinal);
                foreach (var propertyInfo in propertyInfos) {
                    if (!propertyInfo.CanRead)
                        continue;
                    if (!propertyInfo.CanWrite)
                        continue;

#if NETS
                    var attribute = (CfgAttribute)propertyInfo.GetCustomAttribute(typeof(CfgAttribute), true);
#else
                    var attribute = (CfgAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CfgAttribute), true);
#endif

                    if (attribute == null)
                        continue;

                    var key = NormalizeName(type, propertyInfo.Name);
                    var item = new CfgMetadata(propertyInfo, attribute) {
                        TypeDefault = GetDefaultValue(propertyInfo.PropertyType)
                    };

                    // regex
                    try {
                        if (attribute.RegexIsSet) {
#if NETS
                            item.Regex = attribute.ignoreCase ? new Regex(attribute.regex, RegexOptions.IgnoreCase) : new Regex(attribute.regex);
#else
                            item.Regex = attribute.ignoreCase ? new Regex(attribute.regex, RegexOptions.Compiled | RegexOptions.IgnoreCase) : new Regex(attribute.regex, RegexOptions.Compiled);
#endif
                        }
                    } catch (ArgumentException ex) {
                        item.Errors.Add(CfgEvents.InvalidRegex(key, attribute.regex, ex));
                    }

                    // check default value for type mismatch
                    if (attribute.ValueIsSet) {
                        if (attribute.value.GetType() != propertyInfo.PropertyType) {
                            var value = attribute.value;
                            if (TryConvertValue(ref value, propertyInfo.PropertyType)) {
                                attribute.value = value;
                            } else {
                                item.TypeMismatch = true;
                                item.Errors.Add(CfgEvents.TypeMismatch(key, value, propertyInfo.PropertyType));
                            }
                        }
                    }

                    // type safety for value, min value, and max value
                    var defaultValue = attribute.value;
                    if (ResolveType(() => attribute.ValueIsSet, ref defaultValue, key, item)) {
                        attribute.value = defaultValue;
                    }

                    var minValue = attribute.minValue;
                    if (ResolveType(() => attribute.MinValueSet, ref minValue, key, item)) {
                        attribute.minValue = minValue;
                    }

                    var maxValue = attribute.maxValue;
                    if (ResolveType(() => attribute.MaxValueSet, ref maxValue, key, item)) {
                        attribute.maxValue = maxValue;
                    }


                    //foreach (var cp in constructors) {
                    //    if (!cp.Any()) {
                    //        obj = item.ListActivator();
                    //        break;
                    //    }

                    //    if (cp.Count() == 1) {
                    //        if (cp.First().ParameterType == typeof(int)) {
                    //            obj = Activator.CreateInstance(item.ListType, add.Attributes.Count);
                    //            break;
                    //        }

                    //        if (cp.First().ParameterType == typeof(string[])) {
                    //            var names = add.Attributes.Select(a => a.Name).ToArray();
                    //            obj = Activator.CreateInstance(item.ListType, new object[] { names });
                    //            break;
                    //        }
                    //    }
                    //}

#if NETS
                    var propertyTypeInfo = propertyInfo.PropertyType.GetTypeInfo();
                    if (propertyTypeInfo.IsGenericType) {
                        listCache.Add(key);
                        item.ListType = propertyTypeInfo.GenericTypeArguments[0];
                        var listTypeInfo = item.ListType.GetTypeInfo();
                        item.ImplementsProperties = typeof(IProperties).GetTypeInfo().IsAssignableFrom(listTypeInfo);
                        item.Constructors = propertyTypeInfo.DeclaredConstructors.Select(c => c.GetParameters());
                        item.ListActivator = () => Activator.CreateInstance(propertyInfo.PropertyType);
                        if (listTypeInfo.IsSubclassOf(typeof(CfgNode))) {
                            item.Loader = () => (CfgNode)Activator.CreateInstance(item.ListType);
                        }
                    } else {
                        keyCache.Add(key);
                    }
#else
                    if (propertyInfo.PropertyType.IsGenericType) {
                        listCache.Add(key);
                        item.ListType = propertyInfo.PropertyType.GetGenericArguments()[0];
                        item.ImplementsProperties = typeof(IProperties).IsAssignableFrom(item.ListType);
                        item.Constructors = item.ListType.GetConstructors().Select(c => c.GetParameters());
                        item.ListActivator = () => Activator.CreateInstance(propertyInfo.PropertyType);
                        if (item.ListType.IsSubclassOf(typeof(CfgNode))) {
                            
                            item.Loader = () => (CfgNode)Activator.CreateInstance(item.ListType);
                        }
                    } else {
                        keyCache.Add(key);
                    }
#endif

                    if (string.IsNullOrEmpty(attribute.name)) {
                        attribute.name = key;
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

        private static bool ResolveType(Func<bool> isSet, ref object input, string key, CfgMetadata metadata) {
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
            metadata.Errors.Add(CfgEvents.TypeMismatch(key, value, type));
            return false;
        }

        private static object GetDefaultValue(Type t) {
#if NETS
            return t.GetTypeInfo().IsValueType ? Activator.CreateInstance(t) : null;
#else
            return t.IsValueType ? Activator.CreateInstance(t) : null;
#endif
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
                if (NameCache[type].TryGetValue(name, out var value)) {
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
            return PropertyCache.TryGetValue(type, out var names) ? names : new List<string>();
        }

        public static IEnumerable<string> ElementNames(Type type) {
            return ElementCache.TryGetValue(type, out var names) ? names : new List<string>();
        }

        /// <summary>
        /// Clone any node that has a parameterless constructor defined.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static T Clone<T>(T node) where T : CfgNode {

            var clone = Activator.CreateInstance<T>();

            clone.Parser = node.Parser;
            clone.Reader = node.Reader;
            clone.Serializer = node.Serializer;
            clone.Customizers = node.Customizers;
            clone.Type = node.Type;

            var meta = GetMetadata(typeof(T));
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

                    bool isAssignable = false;
#if NETS
                    isAssignable = typeof(CfgNode).GetTypeInfo().IsAssignableFrom(pair.Value.ListType.GetTypeInfo());
#else
                    isAssignable = typeof(CfgNode).IsAssignableFrom(pair.Value.ListType);
#endif
                    if (!isAssignable)
                        continue;

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