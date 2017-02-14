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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cfg.Net.Contracts;
using Cfg.Net.Loggers;
using Cfg.Net.Parsers;
using Cfg.Net.Serializers;

namespace Cfg.Net {
    public abstract class CfgNode {

        static readonly object Locker = new object();

        internal IParser Parser { get; set; }
        internal IReader Reader { get; set; }
        internal ISerializer Serializer { get; set; }
        internal IList<ICustomizer> Customizers { get; set; } = new List<ICustomizer>();
        internal Type Type { get; set; }
        internal CfgEvents Events { get; private set; }
        protected Dictionary<string, string> UniqueProperties { get; } = new Dictionary<string, string>();
        private readonly List<string> _modelErrors = new List<string>();
        protected CfgNode() {
            Events = new CfgEvents(new DefaultLogger(new MemoryLogger()));
            Clear();
            Type = GetType();
        }

        /// <summary>
        /// Constructor for injecting anything marked with IDependency
        /// </summary>
        /// <param name="dependencies"></param>
        protected CfgNode(params IDependency[] dependencies) : this() {

            if (dependencies == null)
                return;

            foreach (var dependency in dependencies.Where(dependency => dependency != null)) {
                if (dependency is IReader) {
                    Reader = dependency as IReader;
                } else if (dependency is IParser) {
                    Parser = dependency as IParser;
                } else if (dependency is ISerializer) {
                    Serializer = dependency as ISerializer;
                } else if (dependency is ILogger) {
                    Events.Logger.AddLogger(dependency as ILogger);
                } else if (dependency is ICustomizer) {
                    Customizers.Add(dependency as ICustomizer);
                }
            }
        }

        protected internal void Error(string message, params object[] args) {
            Events.Error(message, args);
        }

        protected internal void Warn(string message, params object[] args) {
            Events.Warning(message, args);
        }

        /// <summary>
        /// Load the configuration into the root (top-most) node.
        /// </summary>
        /// <param name="cfg">by default, cfg should be XML or JSON, but can be other things depending on what IParser is injected.</param>
        /// <param name="parameters">key, value pairs that may be used in ICustomizer implementations.</param>
        public void Load(string cfg, IDictionary<string, string> parameters = null) {

            Events.Clear(_modelErrors);

            if (string.IsNullOrEmpty(cfg)) {
                Events.NullOrEmptyConfiguration();
                return;
            }

            cfg = cfg.Trim();

            if (parameters == null) {
                parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            INode node;
            try {
                if (Reader != null) {
                    cfg = Reader.Read(cfg, parameters, Events.Logger);
                    if (Events.Logger.Errors().Any()) {
                        return;
                    }
                }

                if (Parser == null) {
                    switch (cfg[0]) {
                        case '{':
                            node = new FastJsonParser().Parse(cfg);
                            break;
                        case '<':
                            node = new NanoXmlParser().Parse(cfg);
                            break;
                        default:
                            Events.InvalidConfiguration(cfg[0]);
                            return;
                    }
                } else {
                    node = Parser.Parse(cfg);
                }
            } catch (Exception ex) {
                Events.ParseException(ex.Message);
                return;
            }

            if (Serializer == null) {
                switch (cfg[0]) {
                    case '{':
                        Serializer = new JsonSerializer();
                        break;
                    default:
                        Serializer = new XmlSerializer();
                        break;
                }
            }

            foreach (var customizer in Customizers) {
                try {
                    customizer.Customize(node, parameters, Events.Logger);
                } catch (Exception ex) {
                    Events.Error($"{customizer.GetType().Name} error: {ex.Message}");
                }
            }

            Process(node, string.Empty, Serializer, Events, parameters, Customizers);
        }

        public void Check() {
            Events.Clear(_modelErrors);
            var name = CfgMetadataCache.NormalizeName(Type, Type.Name);
            var node = new ObjectNode(this, name);
            Process(node, name, Serializer, Events, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), Customizers);
        }
        private CfgNode Process(
            INode node,
            string parent,
            ISerializer serializer,
            CfgEvents events,
            IDictionary<string, string> parameters,
            IList<ICustomizer> customizers
        ) {
            Clear();

            // preserving events, customizers, and serializer
            Events = events;
            Customizers = customizers;
            Serializer = serializer;

            LoadProperties(node, parent, parameters, customizers);
            LoadCollections(node, parent, parameters);
            PreValidate();
            ValidateBasedOnAttributes(node, parameters);
            ValidateListsBasedOnAttributes(node.Name);
            Validate();
            PostValidate();
            return this;
        }

        /// <summary>
        ///     Override to add custom validation.  Use `Error()` or `Warn()` to record issues.
        /// </summary>
        protected internal virtual void Validate() { }

        /// <summary>
        ///     Allows for modification of configuration before validation.
        ///     Note: You are not protected from `null` here.
        /// </summary>
        protected internal virtual void PreValidate() { }

        /// <summary>
        ///     Allows for modification of configuration after validation.
        ///     Note: You can check for Errors() here and modify accordingly.
        /// </summary>
        protected internal virtual void PostValidate() { }

        public string Serialize() {
            return (Serializer ?? new XmlSerializer()).Serialize(this);
        }

        void LoadCollections(INode node, string parentName, IDictionary<string, string> parameters) {
            var metadata = CfgMetadataCache.GetMetadata(Type);
            var elementNames = CfgMetadataCache.ElementNames(Type).ToList();
            var elements = new Dictionary<string, IList>();
            var elementHits = new HashSet<string>();
            var addHits = new HashSet<string>();

            //initialize all the lists
            for (var i = 0; i < elementNames.Count; i++) {
                var key = elementNames[i];
                elements.Add(key, (IList)metadata[key].Getter(this));
            }

            for (var i = 0; i < node.SubNodes.Count; i++) {
                var subNode = node.SubNodes[i];
                var subNodeKey = CfgMetadataCache.NormalizeName(Type, subNode.Name);
                if (metadata.ContainsKey(subNodeKey)) {
                    elementHits.Add(subNodeKey);
                    var item = metadata[subNodeKey];

                    for (var j = 0; j < subNode.SubNodes.Count; j++) {
                        var add = subNode.SubNodes[j];
                        if (add.Name.Equals("add", StringComparison.Ordinal)) {
                            var addKey = CfgMetadataCache.NormalizeName(Type, subNode.Name);
                            addHits.Add(addKey);
                            if (item.Loader == null) {

                                if (item.ImplementsProperties) {
                                    object obj = null;

                                    //todo: these activator.createinstances should be cached
                                    foreach (var cp in item.Constructors) {
                                        if (!cp.Any()) {
                                            obj = Activator.CreateInstance(item.ListType);
                                            break;
                                        }

                                        if (cp.Count() == 1) {
                                            if (cp.First().ParameterType == typeof(int)) {
                                                obj = Activator.CreateInstance(item.ListType, add.Attributes.Count);
                                                break;
                                            }

                                            if (cp.First().ParameterType == typeof(string[])) {
                                                var names = add.Attributes.Select(a => a.Name).ToArray();
                                                obj = Activator.CreateInstance(item.ListType, new object[] { names });
                                                break;
                                            }
                                        }
                                    }
                                    if (obj == null) {
                                        Events.ConstructorNotFound(parentName, subNode.Name);
                                    } else {
                                        var properties = obj as IProperties;
                                        for (var k = 0; k < add.Attributes.Count; k++) {
                                            var attribute = add.Attributes[k];
                                            properties[attribute.Name] = attribute.Value;
                                        }
                                        elements[addKey].Add(obj);
                                    }
                                } else {
                                    if (add.Attributes.Count == 1) {
                                        var attrValue = add.Attributes[0].Value;
                                        if (item.ListType == typeof(string) || item.ListType == typeof(object)) {
                                            elements[addKey].Add(attrValue);
                                        } else {
                                            try {
                                                elements[addKey].Add(CfgConstants.Converter[item.ListType](attrValue));
                                            } catch (Exception ex) {
                                                Events.SettingValue(subNode.Name, attrValue, parentName, subNode.Name, ex.Message);
                                            }
                                        }
                                    } else {
                                        Events.OnlyOneAttributeAllowed(parentName, subNode.Name, add.Attributes.Count);
                                    }
                                }
                            } else {
                                var processed = item.Loader().Process(add, subNode.Name, Serializer, Events, parameters, Customizers);
                                elements[addKey].Add(processed);
                            }
                        } else {
                            Events.UnexpectedElement(add.Name, subNode.Name);
                        }
                    }
                } else {
                    Events.InvalidElement(parentName, node.Name, subNode.Name);
                }
            }
        }

        protected internal void ValidateListsBasedOnAttributes(string parent) {
            var metadata = CfgMetadataCache.GetMetadata(Type);
            var elementNames = CfgMetadataCache.ElementNames(Type).ToList();
            foreach (var listName in elementNames) {
                var listMetadata = metadata[listName];
                var list = (IList)listMetadata.Getter(this);
                ValidateUniqueAndRequiredProperties(parent, listName, listMetadata, list);
            }
        }

        void ValidateUniqueAndRequiredProperties(
            string parent,
            string listName,
            CfgMetadata listMetadata,
            ICollection list
        ) {

            //if more than one then uniqueness comes into question
            if (list.Count > 1) {
                lock (Locker) {
                    if (listMetadata.UniquePropertiesInList == null) {
                        listMetadata.UniquePropertiesInList = CfgMetadataCache.GetMetadata(listMetadata.ListType)
                            .Where(p => p.Value.Attribute.unique)
                            .Select(p => p.Key)
                            .ToArray();
                    }
                }

                if (listMetadata.UniquePropertiesInList.Length <= 0)
                    return;

                for (var j = 0; j < listMetadata.UniquePropertiesInList.Length; j++) {
                    var unique = listMetadata.UniquePropertiesInList[j];
                    var duplicates = list
                        .Cast<CfgNode>()
                        .Where(n => n.UniqueProperties.ContainsKey(unique))
                        .Select(n => n.UniqueProperties[unique])
                        .GroupBy(n => n)
                        .Where(group => @group.Count() > 1)
                        .Select(group => @group.Key)
                        .ToArray();

                    for (var l = 0; l < duplicates.Length; l++) {
                        Events.DuplicateSet(unique, duplicates[l], listName);
                    }
                }
            } else if (list.Count == 0 && listMetadata.Attribute.required) {
                Events.MissingRequiredAdd(parent, listName);
            }
        }

        void LoadProperties(INode node, string parentName, IDictionary<string, string> parameters, IList<ICustomizer> customizers) {
            var metadata = CfgMetadataCache.GetMetadata(Type);
            var keys = CfgMetadataCache.PropertyNames(Type).ToArray();

            if (!keys.Any())
                return;

            var keyHits = new HashSet<string>();
            var nullWarnings = new HashSet<string>();

            // first pass, set default values and report invalid attributes
            foreach (var attribute in node.Attributes) {

                var attributeKey = CfgMetadataCache.NormalizeName(Type, attribute.Name);
                if (metadata.ContainsKey(attributeKey)) {
                    var item = metadata[attributeKey];

                    if (attribute.Value != null)
                        continue;

                    // if attribute is null, use the getter
                    var maybe = item.Getter(this);
                    if (maybe == null) {
                        if (nullWarnings.Add(attribute.Name)) {
                            Events.Warning($"'{attribute.Name}' in '{parentName}' is susceptible to nulls.");
                        }
                        continue;
                    }
                    attribute.Value = maybe;
                } else {
                    Events.InvalidAttribute(parentName, node.Name, attribute.Name, string.Join(", ", keys));
                }
            }

            foreach (var customizer in customizers) {
                try {
                    customizer.Customize(parentName, node, parameters, Events.Logger);
                } catch (Exception ex) {
                    Events.Error($"{customizer.GetType().Name} error: {ex.Message}");
                }
            }

            // second pass, after customizers, adjust case and trim, set property value
            foreach (var attribute in node.Attributes) {
                var attributeKey = CfgMetadataCache.NormalizeName(Type, attribute.Name);
                if (metadata.ContainsKey(attributeKey)) {
                    var item = metadata[attributeKey];

                    if (item.PropertyInfo.PropertyType == typeof(string)) {

                        if (attribute.Value == null)  // user has not provided a default
                            continue;

                        var stringValue = attribute.Value.ToString();

                        if (item.Attribute.toLower) {
                            stringValue = stringValue.ToLower();
                        } else if (item.Attribute.toUpper) {
                            stringValue = stringValue.ToUpper();
                        }

                        if (item.Attribute.trim || item.Attribute.trimStart && item.Attribute.trimEnd) {
                            stringValue = stringValue.Trim();
                        } else {
                            if (item.Attribute.trimStart) {
                                stringValue = stringValue.TrimStart();
                            }
                            if (item.Attribute.trimEnd) {
                                stringValue = stringValue.TrimEnd();
                            }
                        }

                        attribute.Value = stringValue;
                        item.Setter(this, attribute.Value);
                        keyHits.Add(attributeKey);
                    } else {
                        try {
                            item.Setter(this, CfgConstants.Converter[item.PropertyInfo.PropertyType](attribute.Value));
                            keyHits.Add(attributeKey);
                        } catch (Exception ex) {
                            Events.SettingValue(attribute.Name, attribute.Value, parentName, node.Name, ex.Message);
                        }
                    }
                }
            }

            // missing any required attributes?
            foreach (var key in keys.Except(keyHits)) {
                var item = metadata[key];
                if (item.Attribute.required) {
                    Events.MissingAttribute(parentName, node.Name, key);
                }
            }

        }

        internal void ValidateBasedOnAttributes(INode node, IDictionary<string, string> parameters) {
            var metadata = CfgMetadataCache.GetMetadata(Type);
            var keys = CfgMetadataCache.PropertyNames(Type).ToArray();

            if (!keys.Any())
                return;

            for (var i = 0; i < keys.Length; i++) {
                var key = keys[i];
                var item = metadata[key];

                var objectValue = item.Getter(this);
                if (objectValue == null)
                    continue;

                var stringValue = item.PropertyInfo.PropertyType == typeof(string) ? (string)objectValue : objectValue.ToString();

                if (item.Attribute.unique) {
                    UniqueProperties[key] = stringValue;
                }

                if (item.Attribute.DomainSet) {
                    if (!item.IsInDomain(stringValue)) {
                        Events.ValueNotInDomain(key, stringValue, item.Attribute.domain.Replace(item.Attribute.delimiter.ToString(), ", "));
                    }
                }

                CheckValueLength(item.Attribute, key, stringValue);

                CheckValueBoundaries(item.Attribute, key, objectValue);

                CheckValueMatchesRegex(item, key, stringValue);

            }
        }

        void CheckValueMatchesRegex(CfgMetadata metaData, string name, string value) {
            if (!metaData.Attribute.RegexIsSet || metaData.Regex == null) {
                return;
            }

            if (!metaData.Regex.IsMatch(value)) {
                Events.ValueDoesNotMatchRegex(name, value, metaData.Attribute.regex);
            }
        }

        void CheckValueLength(CfgAttribute itemAttributes, string name, string value) {
            if (itemAttributes.MinLengthSet) {
                if (value.Length < itemAttributes.minLength) {
                    Events.ValueTooShort(name, value, itemAttributes.minLength);
                }
            }

            if (!itemAttributes.MaxLengthSet)
                return;

            if (value.Length > itemAttributes.maxLength) {
                Events.ValueTooLong(name, value, itemAttributes.maxLength);
            }
        }

        void CheckValueBoundaries(CfgAttribute itemAttributes, string name, object value) {
            if (!itemAttributes.MinValueSet && !itemAttributes.MaxValueSet)
                return;

            var comparable = value as IComparable;
            if (comparable == null) {
                Events.ValueIsNotComparable(name, value);
            } else {
                if (itemAttributes.MinValueSet) {
                    if (comparable.CompareTo(itemAttributes.minValue) < 0) {
                        Events.ValueTooSmall(name, value, itemAttributes.minValue);
                    }
                }

                if (!itemAttributes.MaxValueSet)
                    return;

                if (comparable.CompareTo(itemAttributes.maxValue) > 0) {
                    Events.ValueTooBig(name, value, itemAttributes.maxValue);
                }
            }
        }

        public string[] Errors() {
            return Events.Logger.Errors();
        }

        public string[] Warnings() {
            return Events.Logger.Warnings();
        }

        /// <summary>
        /// Clears Cfg decorated properties.
        /// </summary>
        private void Clear() {
            var metadata = CfgMetadataCache.GetMetadata(GetType());
            foreach (var pair in metadata) {
                foreach (var error in pair.Value.Errors) {
                    _modelErrors.Add(error);
                }
                if (pair.Value.ListType == null) {
                    pair.Value.Setter(this, pair.Value.TypeMismatch ? pair.Value.TypeDefault : pair.Value.Attribute.value);
                } else {
                    pair.Value.Setter(this, pair.Value.ListActivator());
                }
            }
        }
    }
}