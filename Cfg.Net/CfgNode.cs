#region License
// Cfg-NET An alternative .NET configuration handler.
// Copyright 2015 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using Cfg.Net.Contracts;
using Cfg.Net.Loggers;
using Cfg.Net.Parsers;
using Cfg.Net.Serializers;
using Cfg.Net.Shorthand;

namespace Cfg.Net {

    public abstract class CfgNode {

        private static readonly object Locker = new object();

        private readonly ILogger _logger;
        private IParser _parser;
        private ISerializer _serializer;
        private readonly IReader _reader;
        private readonly Type _type;
        private CfgEvents _events;
        private Dictionary<string, CfgMetadata> _metadata;
        private ShorthandRoot _shorthand;
        private IDictionary<string, IValidator> _validators = new Dictionary<string, IValidator>();

        protected Dictionary<string, string> UniqueProperties { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Constructor for injecting anything marked with IDependency
        /// </summary>
        /// <param name="dependencies"></param>
        protected CfgNode(params IDependency[] dependencies) {
            if (dependencies != null) {
                foreach (var dependency in dependencies.Where(dependency => dependency != null)) {
                    if (dependency is IReader) {
                        _reader = dependency as IReader;
                    } else if (dependency is IParser) {
                        _parser = dependency as IParser;
                    } else if (dependency is ISerializer) {
                        _serializer = dependency as ISerializer;
                    } else if (dependency is ILogger) {
                        _logger = dependency as ILogger;
                    } else if (dependency is IValidators) {
                        foreach (var pair in dependency as IValidators) {
                            _validators[pair.Key] = pair.Value;
                        }
                    }
                }
            }

            _type = GetType();
        }

        internal CfgEvents Events {
            get { return _events ?? (_events = new CfgEvents(new CfgLogger(new MemoryLogger(), _logger))); }
            set { _events = value; }
        }

        /// <summary>
        ///     Get any type that inherits from CfgNode with default values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setter"></param>
        /// <returns></returns>
        public T GetDefaultOf<T>(Action<T> setter = null) {
            var obj = Activator.CreateInstance(typeof(T));

            var metadata = CfgMetadataCache.GetMetadata(typeof(T), Events);
            CfgMetadataCache.SetDefaults(obj, metadata);

            var typed = (T)obj;
            setter?.Invoke(typed);

            ((CfgNode)obj).PreValidate();
            return typed;
        }

        public T GetValidatedOf<T>(Action<T> setter = null) {
            var obj = Activator.CreateInstance(typeof(T));

            var metadata = CfgMetadataCache.GetMetadata(typeof(T), Events);
            CfgMetadataCache.SetDefaults(obj, metadata);

            var typed = (T)obj;
            setter?.Invoke(typed);

            var node = (CfgNode)obj;
            node.PreValidate();
            node.ValidateProperties(metadata, null);
            node.Validate();
            node.PostValidate();

            if (!node.Errors().Any())
                return typed;

            foreach (var error in node.Errors()) {
                Events.Error(error);
            }

            return typed;
        }

        protected void Error(string message, params object[] args) {
            Events.Error(message, args);
        }

        protected void Warn(string message, params object[] args) {
            Events.Warning(message, args);
        }

        protected void LoadShorthand(string cfg) {
            _shorthand = new ShorthandRoot(cfg, _reader, _parser);

            if (_shorthand.Warnings().Any()) {
                foreach (var warning in _shorthand.Warnings()) {
                    Events.Warning(warning);
                }
            }

            if (_shorthand.Errors().Any()) {
                foreach (var error in _shorthand.Errors()) {
                    Events.Error(error);
                }
                return;
            }

            _shorthand.InitializeMethodDataLookup();
        }

        /// <summary>
        /// This method is used to load the configuration into the root, or top-most node.
        /// </summary>
        /// <param name="cfg">by default, cfg should be XML or JSON, but can be other things depending on what IParser is injected.</param>
        /// <param name="parameters">key, value pairs that replace @(PlaceHolders) with values.</param>
        public void Load(string cfg, Dictionary<string, string> parameters = null) {
            _metadata = CfgMetadataCache.GetMetadata(_type, Events);
            CfgMetadataCache.SetDefaults(this, _metadata);

            INode node;
            try {
                var sourceDetector = new CfgSourceDetector();
                Source source;
                if (_reader == null) {
                    source = sourceDetector.Detect(cfg, Events.Logger);
                } else {
                    var result = _reader.Read(cfg, Events.Logger);
                    if (Events.Errors().Any()) {
                        return;
                    }
                    cfg = result.Content;
                    source = result.Source;
                    if (result.Source != Source.Error && result.Parameters.Any()) {
                        if (parameters == null) {
                            parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }
                        foreach (var pair in result.Parameters) {
                            parameters[pair.Key] = pair.Value;
                        }
                    }
                }

                if (source == Source.Error) {
                    return;
                }

                SetDefaultParser(source);
                SetDefaultSerializer(cfg, source, sourceDetector);

                node = _parser.Parse(cfg);

                var environmentDefaults = LoadEnvironment(node, parameters).ToArray();
                if (environmentDefaults.Length > 0) {
                    if (parameters == null) {
                        parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    for (var i = 0; i < environmentDefaults.Length; i++) {
                        if (!parameters.ContainsKey(environmentDefaults[i][0])) {
                            parameters[environmentDefaults[i][0]] = environmentDefaults[i][1];
                        }
                    }
                }
            } catch (Exception ex) {
                Events.ParseException(ex.Message);
                return;
            }

            LoadProperties(node, null, _parser, parameters);
            LoadCollections(node, null, _parser, _serializer, parameters);
            PreValidate();
            ValidateProperties(_metadata, null);
            Validate();
            PostValidate();
        }

        private void SetDefaultSerializer(string cfg, Source source, ISourceDetector sourceDetector) {

            if (_serializer != null)
                return;

            switch (source) {
                case Source.Json:
                    _serializer = new JsonSerializer();
                    break;
                case Source.Xml:
                    _serializer = new XmlSerializer();
                    break;
                case Source.File:
                case Source.Url:
                    if (sourceDetector.Detect(cfg, new NullLogger()) == Source.Json) {
                        _serializer = new JsonSerializer();
                    } else {
                        _serializer = new XmlSerializer();
                    }
                    break;
                default:
                    _serializer = new XmlSerializer();
                    break;
            }
        }

        private void SetDefaultParser(Source source) {
            if (_parser != null)
                return;

            if (source == Source.Json) {
                _parser = new FastJsonParser();
            } else {
                _parser = new NanoXmlParser();
            }
        }

        protected IEnumerable<string[]> LoadEnvironment(INode node, Dictionary<string, string> parameters) {
            for (var i = 0; i < node.SubNodes.Count; i++) {
                var environmentsNode = node.SubNodes.FirstOrDefault(n => n.Name == CfgConstants.EnvironmentsElementName);
                if (environmentsNode == null)
                    continue;

                if (environmentsNode.SubNodes.Count == 0)
                    break;

                INode environmentNode;

                if (environmentsNode.SubNodes.Count > 1) {
                    IAttribute defaultEnvironment;
                    if (!node.TryAttribute(CfgConstants.EnvironmentsDefaultName, out defaultEnvironment))
                        continue;

                    for (var j = 0; j < environmentsNode.SubNodes.Count; j++) {
                        environmentNode = environmentsNode.SubNodes[j];

                        IAttribute environmentName;
                        if (!environmentNode.TryAttribute("name", out environmentName))
                            continue;

                        var value = CheckParameters(parameters, defaultEnvironment.Value);

                        if (!value.Equals(environmentName.Value) || environmentNode.SubNodes.Count == 0)
                            continue;

                        return GetParameters(environmentNode.SubNodes[0]);
                    }
                }

                environmentNode = environmentsNode.SubNodes[0];
                if (environmentNode.SubNodes.Count == 0)
                    break;

                var parametersNode = environmentNode.SubNodes[0];

                if (parametersNode.Name != CfgConstants.ParametersElementName || environmentNode.SubNodes.Count == 0)
                    break;

                return GetParameters(parametersNode);
            }

            return Enumerable.Empty<string[]>();
        }

        private static IEnumerable<string[]> GetParameters(INode parametersNode) {
            var parameters = new List<string[]>();

            for (var j = 0; j < parametersNode.SubNodes.Count; j++) {
                var parameter = parametersNode.SubNodes[j];
                string name = null;
                string value = null;
                for (var k = 0; k < parameter.Attributes.Count; k++) {
                    var attribute = parameter.Attributes[k];
                    switch (attribute.Name) {
                        case "name":
                            name = attribute.Value;
                            break;
                        case "value":
                            value = attribute.Value;
                            break;
                    }
                }
                if (name != null && value != null) {
                    parameters.Add(new[] { name, value });
                }
            }
            return parameters;
        }

        private CfgNode Load(
            INode node,
            string parentName,
            IParser parser,
            ISerializer serializer,
            CfgEvents events,
            IDictionary<string, IValidator> validators,
            ShorthandRoot shorthand,
            Dictionary<string, string> parameters
        ) {
            Events = events;
            _shorthand = shorthand;
            _validators = validators;
            _parser = parser;
            _serializer = serializer;
            _metadata = CfgMetadataCache.GetMetadata(_type, events);

            CfgMetadataCache.SetDefaults(this, _metadata);
            LoadProperties(node, parentName, parser, parameters);
            LoadCollections(node, parentName, parser, serializer, parameters);
            PreValidate();
            ValidateProperties(_metadata, parentName);
            Validate();
            PostValidate();
            return this;
        }

        /// <summary>
        ///     Override to add custom validation.  Use `Error()` or `Warn()` to record issues.
        /// </summary>
        protected virtual void Validate() {
        }

        /// <summary>
        ///     Allows for modification of configuration before validation.
        ///     Note: You are not protected from `null` here.
        /// </summary>
        protected virtual void PreValidate() {
        }

        /// <summary>
        ///     Allows for modification of configuration after validation.
        ///     Note: You can check for Errors() here and modify accordingly.
        /// </summary>
        protected virtual void PostValidate() {
        }

        public string Serialize() {
            return _serializer.Serialize(this);
        }

        public object Clone() {
            return CfgMetadataCache.Clone(this);
        }

        private void LoadCollections(INode node, string parentName, IParser parser, ISerializer serializer, Dictionary<string, string> parameters = null) {
            var keys = CfgMetadataCache.ElementNames(_type).ToList();
            var elements = new Dictionary<string, IList>();
            var elementHits = new HashSet<string>();
            var addHits = new HashSet<string>();

            //initialize all the lists
            for (var i = 0; i < keys.Count; i++) {
                var key = keys[i];
                elements.Add(key, (IList)_metadata[key].Getter(this));
            }

            for (var i = 0; i < node.SubNodes.Count; i++) {
                var subNode = node.SubNodes[i];
                var subNodeKey = CfgMetadataCache.NormalizeName(_type, subNode.Name);
                if (_metadata.ContainsKey(subNodeKey)) {
                    elementHits.Add(subNodeKey);
                    var item = _metadata[subNodeKey];

                    for (var j = 0; j < subNode.SubNodes.Count; j++) {
                        var add = subNode.SubNodes[j];
                        if (add.Name.Equals("add", StringComparison.Ordinal)) {
                            var addKey = CfgMetadataCache.NormalizeName(_type, subNode.Name);
                            addHits.Add(addKey);
                            if (item.Loader == null) {
                                if (item.ListType == typeof(Dictionary<string, string>)) {
                                    var dict = new Dictionary<string, string>();
                                    for (var k = 0; k < add.Attributes.Count; k++) {
                                        var attribute = add.Attributes[k];
                                        dict[attribute.Name] = attribute.Value;
                                    }
                                    elements[addKey].Add(dict);
                                } else {
                                    if (add.Attributes.Count == 1) {
                                        string attrValue = add.Attributes[0].Value;
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
                                var loaded = item.Loader().Load(add, subNode.Name, parser, serializer, Events, _validators, _shorthand, parameters);
                                elements[addKey].Add(loaded);
                            }
                        } else {
                            Events.UnexpectedElement(add.Name, subNode.Name);
                        }
                    }
                } else {
                    if (parentName == null) {
                        Events.InvalidElement(node.Name, subNode.Name);
                    } else {
                        Events.InvalidNestedElement(parentName, node.Name, subNode.Name);
                    }
                }
            }

            // check for duplicates of unique properties required to be unique in collections
            for (int i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var item = _metadata[key];
                var list = elements[key];

                if (list.Count > 1) {
                    lock (Locker) {
                        if (item.UniquePropertiesInList == null) {
                            item.UniquePropertiesInList = CfgMetadataCache.GetMetadata(item.ListType, Events)
                                .Where(p => p.Value.Attribute.unique)
                                .Select(p => p.Key)
                                .ToArray();
                        }
                    }

                    if (item.UniquePropertiesInList.Length <= 0)
                        continue;

                    for (var j = 0; j < item.UniquePropertiesInList.Length; j++) {
                        var unique = item.UniquePropertiesInList[j];
                        var duplicates = list
                            .Cast<CfgNode>()
                            .Where(n => n.UniqueProperties.ContainsKey(unique))
                            .Select(n => n.UniqueProperties[unique])
                            .GroupBy(n => n)
                            .Where(group => @group.Count() > 1)
                            .Select(group => @group.Key)
                            .ToArray();

                        for (var l = 0; l < duplicates.Length; l++) {
                            Events.DuplicateSet(unique, duplicates[l], key);
                        }
                    }
                } else if (list.Count == 0 && item.Attribute.required) {
                    if (elementHits.Contains(key) && !addHits.Contains(key)) {
                        Events.MissingAddElement(key);
                    } else {
                        if (parentName == null) {
                            Events.MissingElement(node.Name, key);
                        } else {
                            Events.MissingNestedElement(parentName, node.Name, key);
                        }
                    }
                }
            }
        }

        private void LoadProperties(INode node, string parentName, IParser parser, IDictionary<string, string> parameters = null) {
            var keys = CfgMetadataCache.PropertyNames(_type).ToArray();

            if (!keys.Any())
                return;

            var keyHits = new HashSet<string>();
            var nullWarnings = new HashSet<string>();

            for (var i = 0; i < node.Attributes.Count; i++) {
                var attribute = node.Attributes[i];
                var attributeKey = CfgMetadataCache.NormalizeName(_type, attribute.Name);
                if (_metadata.ContainsKey(attributeKey)) {
                    var item = _metadata[attributeKey];

                    // if attribute is null and no default is set, try the getter
                    if (attribute.Value == null && item.Attribute.value == null) {
                        var maybe = item.Getter(this);
                        if (maybe == null) {
                            if (nullWarnings.Add(attribute.Name)) {
                                _logger.Warn("'{0}' in '{1}' is susceptible to nulls.", attribute.Name, parentName);
                            }
                            continue;
                        }
                        attribute.Value = maybe.ToString();
                    }

                    attribute.Value = parser.Decode(attribute.Value);
                    attribute.Value = CheckParameters(parameters, attribute.Value);

                    if (item.Attribute.toLower) {
                        attribute.Value = attribute.Value.ToLower();
                    } else if (item.Attribute.toUpper) {
                        attribute.Value = attribute.Value.ToUpper();
                    }

                    if (item.Attribute.shorthand) {
                        if (_shorthand?.MethodDataLookup == null) {
                            Events.ShorthandNotLoaded(parentName, node.Name, attribute.Name);
                        } else {
                            TranslateShorthand(node, attribute.Value);
                        }
                    }

                    if (item.PropertyInfo.PropertyType == typeof(string)) {
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
                } else {
                    Events.InvalidAttribute(parentName, node.Name, attribute.Name, string.Join(", ", keys));
                }
            }

            // missing any required attributes?
            foreach (var key in keys.Except(keyHits)) {
                var item = _metadata[key];
                if (item.Attribute.required) {
                    Events.MissingAttribute(parentName, node.Name, key);
                }
            }

        }

        private void ValidateProperties(IDictionary<string, CfgMetadata> metadata, string parentName) {
            var keys = CfgMetadataCache.PropertyNames(_type).ToArray();

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
                        if (parentName == null) {
                            Events.RootValueNotInDomain(stringValue, key, item.Attribute.domain.Replace(item.Attribute.domainDelimiter.ToString(), ", "));
                        } else {
                            Events.ValueNotInDomain(parentName, key, stringValue, item.Attribute.domain.Replace(item.Attribute.domainDelimiter.ToString(), ", "));
                        }
                    }
                }

                CheckValueLength(item.Attribute, key, stringValue);

                CheckValueBoundaries(item.Attribute, key, objectValue);

                CheckValidators(item, parentName, key, objectValue);
            }
        }

        private void CheckValidators(CfgMetadata item, string parent, string name, object value) {
            if (!item.Attribute.ValidatorsSet)
                return;

            foreach (var validator in item.Validators()) {
                if (_validators.ContainsKey(validator)) {
                    try {
                        var result = _validators[validator].Validate(parent, name, value);
                        if (result.Valid)
                            continue;

                        if (result.Warnings != null) {
                            foreach (var warning in result.Warnings) {
                                Warn(warning);
                            }
                        }
                        if (result.Errors != null) {
                            foreach (var error in result.Errors) {
                                Error(error);
                            }
                        }
                    } catch (Exception ex) {
                        Events.ValidatorException(validator, ex, value);
                    }
                } else {
                    Events.MissingValidator(parent, name, validator);
                }
            }
        }

        private void TranslateShorthand(INode node, string stringValue) {
            var expressions = new Expressions(stringValue);
            var shorthandNodes = new Dictionary<string, List<INode>>();

            for (int j = 0; j < expressions.Count; j++) {
                Expression expression = expressions[j];
                if (_shorthand.MethodDataLookup.ContainsKey(expression.Method)) {
                    MethodData methodData = _shorthand.MethodDataLookup[expression.Method];
                    var shorthandNode = new ShorthandNode("add");
                    shorthandNode.Attributes.Add(new ShorthandAttribute(methodData.Target.Property, expression.Method));

                    List<Parameter> signatureParameters =
                        methodData.Signature.Parameters.Select(p => new Parameter { Name = p.Name, Value = p.Value })
                            .ToList();
                    string[] passedParameters = expression.Parameters.Select(p => new string(p.ToCharArray())).ToArray();

                    // single parameters
                    if (methodData.Signature.Parameters.Count == 1 && expression.SingleParameter != string.Empty) {
                        string name = methodData.Signature.Parameters[0].Name;
                        string value = expression.SingleParameter.StartsWith(name + ":",
                            StringComparison.OrdinalIgnoreCase)
                            ? expression.SingleParameter.Remove(0, name.Length + 1)
                            : expression.SingleParameter;
                        shorthandNode.Attributes.Add(new ShorthandAttribute(name, value));
                    } else {
                        // named parameters
                        for (int i = 0; i < passedParameters.Length; i++) {
                            string parameter = passedParameters[i];
                            string[] split = Split(parameter, CfgConstants.NamedParameterSplitter);
                            if (split.Length == 2) {
                                string name = CfgMetadataCache.NormalizeName(typeof(Parameter), split[0]);
                                shorthandNode.Attributes.Add(new ShorthandAttribute(name, split[1]));
                                signatureParameters.RemoveAll(
                                    p => CfgMetadataCache.NormalizeName(typeof(Parameter), p.Name) == name);
                                expression.Parameters.RemoveAll(p => p == parameter);
                            }
                        }

                        // ordered nameless parameters
                        for (int m = 0; m < signatureParameters.Count; m++) {
                            Parameter signatureParameter = signatureParameters[m];
                            shorthandNode.Attributes.Add(m < expression.Parameters.Count
                                ? new ShorthandAttribute(signatureParameter.Name, expression.Parameters[m])
                                : new ShorthandAttribute(signatureParameter.Name, signatureParameter.Value));
                        }
                    }

                    if (shorthandNodes.ContainsKey(methodData.Target.Collection)) {
                        shorthandNodes[methodData.Target.Collection].Add(shorthandNode);
                    } else {
                        shorthandNodes[methodData.Target.Collection] = new List<INode> { shorthandNode };
                    }
                }
            }

            foreach (var pair in shorthandNodes) {
                INode shorthandCollection = node.SubNodes.FirstOrDefault(sn => sn.Name == pair.Key);
                if (shorthandCollection == null) {
                    shorthandCollection = new ShorthandNode(pair.Key);
                    shorthandCollection.SubNodes.AddRange(pair.Value);
                    node.SubNodes.Add(shorthandCollection);
                } else {
                    shorthandCollection.SubNodes.InsertRange(0, pair.Value);
                }
            }
        }

        private void CheckValueLength(CfgAttribute itemAttributes, string name, string value) {
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

        private void CheckValueBoundaries(CfgAttribute itemAttributes, string name, object value) {
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

        private string CheckParameters(IDictionary<string, string> parameters, string input) {
            if (parameters == null || input.IndexOf('@') < 0)
                return input;
            var response = ReplaceParameters(input, parameters);
            if (response.Item2.Length > 1) {
                Events.MissingPlaceHolderValues(response.Item2);
            }
            return response.Item1;
        }

        private static Tuple<string, string[]> ReplaceParameters(string value, IDictionary<string, string> parameters) {
            var builder = new StringBuilder();
            List<string> badKeys = null;
            for (int j = 0; j < value.Length; j++) {
                if (value[j] == CfgConstants.PlaceHolderFirst &&
                    value.Length > j + 1 &&
                    value[j + 1] == CfgConstants.PlaceHolderSecond) {
                    int length = 2;
                    while (value.Length > j + length && value[j + length] != CfgConstants.PlaceHolderLast) {
                        length++;
                    }
                    if (length > 2) {
                        string key = value.Substring(j + 2, length - 2);
                        if (parameters.ContainsKey(key)) {
                            builder.Append(parameters[key]);
                        } else {
                            if (badKeys == null) {
                                badKeys = new List<string> { key };
                            } else {
                                badKeys.Add(key);
                            }
                            builder.AppendFormat("@({0})", key);
                        }
                    }
                    j = j + length;
                } else {
                    builder.Append(value[j]);
                }
            }
            return new Tuple<string, string[]>(builder.ToString(), badKeys == null ? new string[0] : badKeys.ToArray());
        }

        public List<string> Logs() {
            var list = new List<string>(Errors());
            list.AddRange(Warnings());
            return list;
        }

        public string[] Errors() {
            return Events.Errors();
        }

        public string[] Warnings() {
            return Events.Warnings();
        }

        internal static string[] Split(string arg, char splitter, int skip = 0) {
            if (arg.Equals(string.Empty))
                return new string[0];

            string[] split = arg.Replace("\\" + splitter, CfgConstants.ControlString).Split(splitter);
            return
                split.Select(s => s.Replace(CfgConstants.ControlChar, splitter))
                    .Skip(skip)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }

        internal static string[] Split(string arg, string[] splitter, int skip = 0) {
            if (arg.Equals(string.Empty))
                return new string[0];

            string[] split = arg.Replace("\\" + splitter[0], CfgConstants.ControlString).Split(splitter, StringSplitOptions.None);
            return
                split.Select(s => s.Replace(CfgConstants.ControlString, splitter[0]))
                    .Skip(skip)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }
    }
}