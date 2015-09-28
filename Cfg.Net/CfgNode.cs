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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Cfg.Net.Contracts;
using Cfg.Net.Loggers;
using Cfg.Net.Parsers;
using Cfg.Net.Shorthand;

namespace Cfg.Net {

    public abstract class CfgNode {

        internal static string ControlString = ((char)31).ToString();
        internal static char ControlChar = (char)31;
        internal static char NamedParameterSplitter = ':';

        //shared cache
        private static bool _initialized;
        private static readonly object Locker = new object();
        private static Dictionary<Type, Dictionary<string, CfgMetadata>> _metadataCache;
        private static Dictionary<Type, List<string>> _propertyCache;
        private static Dictionary<Type, List<string>> _elementCache;
        private static Dictionary<Type, Dictionary<string, string>> _nameCache;
        private static Dictionary<Type, Func<string, object>> _converter;

        private readonly ILogger _logger;
        private readonly IParser _parser;
        private readonly IReader _reader;
        private readonly Type _type;
        private CfgEvents _events;
        private Dictionary<string, CfgMetadata> _metadata;
        private ShorthandRoot _shorthand;
        private IDictionary<string, IValidator> _validators = new Dictionary<string, IValidator>();

        protected Dictionary<string, string> UniqueProperties { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Fancy constructor for injecting anything marked as an IDependency
        /// </summary>
        /// <param name="dependencies"></param>
        protected CfgNode(params IDependency[] dependencies) {
            if (dependencies != null) {
                foreach (var dependency in dependencies.Where(dependency => dependency != null)) {
                    if (dependency is IReader) {
                        _reader = dependency as IReader;
                    } else if (dependency is IParser) {
                        _parser = dependency as IParser;
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
            lock (Locker) {
                if (_initialized)
                    return;
                Initialize();
                _initialized = true;
            }
        }

        internal CfgEvents Events {
            get { return _events ?? (_events = new CfgEvents(new CfgLogger(new MemoryLogger(), _logger))); }
            set { _events = value; }
        }

        private static void Initialize() {
            _metadataCache = new Dictionary<Type, Dictionary<string, CfgMetadata>>();
            _propertyCache = new Dictionary<Type, List<string>>();
            _elementCache = new Dictionary<Type, List<string>>();
            _nameCache = new Dictionary<Type, Dictionary<string, string>>();
            _converter = new Dictionary<Type, Func<string, object>> {
                {typeof (string), (x => x)},
                {typeof (Guid), (x => Guid.Parse(x))},
                {typeof (short), (x => Convert.ToInt16(x))},
                {typeof (int), (x => Convert.ToInt32(x))},
                {typeof (long), (x => Convert.ToInt64(x))},
                {typeof (ushort), (x => Convert.ToUInt16(x))},
                {typeof (uint), (x => Convert.ToUInt32(x))},
                {typeof (ulong), (x => Convert.ToUInt64(x))},
                {typeof (double), (x => Convert.ToDouble(x))},
                {typeof (decimal), (x => decimal.Parse(x, NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof (NumberFormatInfo))))},
                {typeof (char), (x => Convert.ToChar(x))},
                {typeof (DateTime), (x => Convert.ToDateTime(x))},
                {typeof (bool), (x => Convert.ToBoolean(x))},
                {typeof (float), (x => Convert.ToSingle(x))},
                {typeof (byte), (x => Convert.ToByte(x))},
                {typeof(object), (x => x)}
            };
        }

        /// <summary>
        ///     Get any type that inherits from CfgNode with default values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setter"></param>
        /// <returns></returns>
        public T GetDefaultOf<T>(Action<T> setter = null) {
            var obj = Activator.CreateInstance(typeof(T));

            var metadata = GetMetadata(typeof(T), Events);
            SetDefaults(obj, metadata);

            var typed = (T)obj;
            setter?.Invoke(typed);

            ((CfgNode)obj).PreValidate();
            return typed;
        }

        public T GetValidatedOf<T>(Action<T> setter = null) {
            var obj = Activator.CreateInstance(typeof(T));

            var metadata = GetMetadata(typeof(T), Events);
            SetDefaults(obj, metadata);

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

        private static void SetDefaults(object node, Dictionary<string, CfgMetadata> metadata) {
            foreach (var pair in metadata) {
                if (pair.Value.PropertyInfo.PropertyType.IsGenericType) {
                    pair.Value.Setter(node, Activator.CreateInstance(pair.Value.PropertyInfo.PropertyType));
                } else {
                    if (!pair.Value.TypeMismatch) {
                        pair.Value.Setter(node, pair.Value.Attribute.value);
                    }
                }
            }
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

        public void Load(string cfg, Dictionary<string, string> parameters = null) {
            _metadata = GetMetadata(_type, Events);
            SetDefaults(this, _metadata);

            INode node;
            try {
                Source source;

                if (_reader == null) {
                    source = new CfgSourceDetector().Detect(cfg, Events.Logger);
                } else {
                    var result = _reader.Read(cfg, Events.Logger);
                    if (Events.Errors().Any()) {
                        return;
                    }
                    cfg = result.Content;
                    source = result.Source;
                    if (source != Source.Error && result.Parameters.Any()) {
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

                var parser = _parser ?? (source == Source.Json ? new FastJsonParser() : (IParser)new NanoXmlParser());
                node = parser.Parse(cfg);

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

            LoadProperties(node, null, parameters);
            LoadCollections(node, null, parameters);
            PreValidate();
            ValidateProperties(_metadata, null);
            Validate();
            PostValidate();
        }

        protected IEnumerable<string[]> LoadEnvironment(INode node, Dictionary<string, string> parameters) {
            for (int i = 0; i < node.SubNodes.Count; i++) {
                var environmentsNode = node.SubNodes.FirstOrDefault(n=>n.Name == CfgConstants.ENVIRONMENTS_ELEMENT_NAME);
                if (environmentsNode == null)
                    continue;

                if (environmentsNode.SubNodes.Count == 0)
                    break;

                INode environmentNode;

                if (environmentsNode.SubNodes.Count > 1) {
                    IAttribute defaultEnvironment;
                    if (!node.TryAttribute(CfgConstants.ENVIRONMENTS_DEFAULT_NAME, out defaultEnvironment))
                        continue;

                    for (int j = 0; j < environmentsNode.SubNodes.Count; j++) {
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

                if (parametersNode.Name != CfgConstants.PARAMETERS_ELEMENT_NAME || environmentNode.SubNodes.Count == 0)
                    break;

                return GetParameters(parametersNode);
            }

            return Enumerable.Empty<string[]>();
        }

        private static IEnumerable<string[]> GetParameters(INode parametersNode) {
            var parameters = new List<string[]>();

            for (int j = 0; j < parametersNode.SubNodes.Count; j++) {
                INode parameter = parametersNode.SubNodes[j];
                string name = null;
                string value = null;
                for (int k = 0; k < parameter.Attributes.Count; k++) {
                    IAttribute attribute = parameter.Attributes[k];
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

        private CfgNode Load(INode node, string parentName, CfgEvents events, IDictionary<string, IValidator> validators, ShorthandRoot shorthand, Dictionary<string, string> parameters) {
            Events = events;
            _shorthand = shorthand;
            _validators = validators;
            _metadata = GetMetadata(_type, events);
            SetDefaults(this, _metadata);
            LoadProperties(node, parentName, parameters);
            LoadCollections(node, parentName, parameters);
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

        private void LoadCollections(INode node, string parentName, Dictionary<string, string> parameters = null) {
            List<string> keys = _elementCache[_type];
            var elements = new Dictionary<string, IList>();
            var elementHits = new HashSet<string>();
            var addHits = new HashSet<string>();

            //initialize all the lists
            for (int i = 0; i < keys.Count; i++) {
                string key = keys[i];
                elements.Add(key, (IList)_metadata[key].Getter(this));
            }

            for (int i = 0; i < node.SubNodes.Count; i++) {
                INode subNode = node.SubNodes[i];
                string subNodeKey = NormalizeName(_type, subNode.Name);
                if (_metadata.ContainsKey(subNodeKey)) {
                    elementHits.Add(subNodeKey);
                    var item = _metadata[subNodeKey];

                    for (int j = 0; j < subNode.SubNodes.Count; j++) {
                        INode add = subNode.SubNodes[j];
                        if (add.Name.Equals("add", StringComparison.Ordinal)) {
                            string addKey = NormalizeName(_type, subNode.Name);
                            addHits.Add(addKey);
                            if (item.Loader == null) {
                                if (item.ListType == typeof(Dictionary<string, string>)) {
                                    var dict = new Dictionary<string, string>();
                                    for (int k = 0; k < add.Attributes.Count; k++) {
                                        IAttribute attribute = add.Attributes[k];
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
                                                elements[addKey].Add(_converter[item.ListType](attrValue));
                                            } catch (Exception ex) {
                                                Events.SettingValue(subNode.Name, attrValue, parentName, subNode.Name, ex.Message);
                                            }
                                        }
                                    } else {
                                        Events.OnlyOneAttributeAllowed(parentName, subNode.Name, add.Attributes.Count);
                                    }
                                }
                            } else {
                                var loaded = item.Loader().Load(add, subNode.Name, Events, _validators, _shorthand, parameters);
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
                string key = keys[i];
                CfgMetadata item = _metadata[key];
                IList list = elements[key];

                if (list.Count > 1) {
                    lock (Locker) {
                        if (item.UniquePropertiesInList == null) {
                            item.UniquePropertiesInList = GetMetadata(item.ListType, Events)
                                .Where(p => p.Value.Attribute.unique)
                                .Select(p => p.Key)
                                .ToArray();
                        }
                    }

                    if (item.UniquePropertiesInList.Length <= 0)
                        continue;

                    for (int j = 0; j < item.UniquePropertiesInList.Length; j++) {
                        string unique = item.UniquePropertiesInList[j];
                        string[] duplicates = list
                            .Cast<CfgNode>()
                            .Where(n => n.UniqueProperties.ContainsKey(unique))
                            .Select(n => n.UniqueProperties[unique])
                            .GroupBy(n => n)
                            .Where(group => @group.Count() > 1)
                            .Select(group => @group.Key)
                            .ToArray();

                        for (int l = 0; l < duplicates.Length; l++) {
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

        private static string NormalizeName(Type type, string name) {
            var cache = _nameCache[type];
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

        private void LoadProperties(INode node, string parentName, IDictionary<string, string> parameters = null) {
            var keys = _propertyCache[_type];

            if (keys.Count == 0)
                return;

            var keyHits = new HashSet<string>();
            var nullWarnings = new HashSet<string>();

            for (var i = 0; i < node.Attributes.Count; i++) {
                var attribute = node.Attributes[i];
                var attributeKey = NormalizeName(_type, attribute.Name);
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

                    if (attribute.Value.IndexOf(CfgConstants.ENTITY_START) > -1) {
                        attribute.Value = Decode(attribute.Value);
                    }

                    attribute.Value = CheckParameters(parameters, attribute.Value);

                    if (item.Attribute.toLower) {
                        attribute.Value = attribute.Value.ToLower();
                    } else if (item.Attribute.toUpper) {
                        attribute.Value = attribute.Value.ToUpper();
                    }

                    if (item.Attribute.shorthand) {
                        if (_shorthand == null || _shorthand.MethodDataLookup == null) {
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
                            item.Setter(this, _converter[item.PropertyInfo.PropertyType](attribute.Value));
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
            var keys = _propertyCache[_type];

            if (keys.Count == 0)
                return;

            for (var i = 0; i < keys.Count; i++) {
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
                            string[] split = Split(parameter, NamedParameterSplitter);
                            if (split.Length == 2) {
                                string name = NormalizeName(typeof(Parameter), split[0]);
                                shorthandNode.Attributes.Add(new ShorthandAttribute(name, split[1]));
                                signatureParameters.RemoveAll(
                                    p => NormalizeName(typeof(Parameter), p.Name) == name);
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
                if (value[j] == CfgConstants.PLACE_HOLDER_FIRST &&
                    value.Length > j + 1 &&
                    value[j + 1] == CfgConstants.PLACE_HOLDER_SECOND) {
                    int length = 2;
                    while (value.Length > j + length && value[j + length] != CfgConstants.PLACE_HOLDER_LAST) {
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

        private static Dictionary<string, CfgMetadata> GetMetadata(Type type, CfgEvents events) {
            Dictionary<string, CfgMetadata> metadata;

            if (_metadataCache.TryGetValue(type, out metadata))
                return metadata;

            lock (Locker) {
                _nameCache[type] = new Dictionary<string, string>();

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
                                events.TypeMismatch(key, value, propertyInfo.PropertyType);
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

                _propertyCache[type] = keyCache;
                _elementCache[type] = listCache;
                _metadataCache[type] = metadata;
            }

            return _metadataCache[type];
        }

        private static bool ResolveType(Func<bool> isSet, ref object input, string key, CfgMetadata metadata,
            CfgEvents events) {
            if (!isSet())
                return true;

            Type type = metadata.PropertyInfo.PropertyType;

            if (input.GetType() == type)
                return true;

            object value = input;
            if (TryConvertValue(ref value, type)) {
                input = value;
                return true;
            }

            metadata.TypeMismatch = true;
            events.TypeMismatch(key, value, type);
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

        internal static string[] Split(string arg, char splitter, int skip = 0) {
            if (arg.Equals(string.Empty))
                return new string[0];

            string[] split = arg.Replace("\\" + splitter, ControlString).Split(splitter);
            return
                split.Select(s => s.Replace(ControlChar, splitter))
                    .Skip(skip)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }

        internal static string[] Split(string arg, string[] splitter, int skip = 0) {
            if (arg.Equals(string.Empty))
                return new string[0];

            string[] split = arg.Replace("\\" + splitter[0], ControlString).Split(splitter, StringSplitOptions.None);
            return
                split.Select(s => s.Replace(ControlString, splitter[0]))
                    .Skip(skip)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }

        // a naive implementation for hand-written configurations
        public static string Encode(string value) {
            var builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++) {
                char ch = value[0];
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

        public static string Decode(string input) {
            var builder = new StringBuilder();
            var htmlEntityEndingChars = new[] { CfgConstants.ENTITY_END, CfgConstants.ENTITY_START };

            for (int i = 0; i < input.Length; i++) {
                char c = input[i];

                if (c == CfgConstants.ENTITY_START) {
                    // Found &. Look for the next ; or &. If & occurs before ;, then this is not entity, and next & may start another entity
                    int index = input.IndexOfAny(htmlEntityEndingChars, i + 1);
                    if (index > 0 && input[index] == CfgConstants.ENTITY_END) {
                        string entity = input.Substring(i + 1, index - i - 1);

                        if (entity.Length > 1 && entity[0] == '#') {
                            bool parsedSuccessfully;
                            uint parsedValue;
                            if (entity[1] == 'x' || entity[1] == 'X') {
                                parsedSuccessfully = uint.TryParse(entity.Substring(2), NumberStyles.AllowHexSpecifier,
                                    NumberFormatInfo.InvariantInfo, out parsedValue);
                            } else {
                                parsedSuccessfully = uint.TryParse(entity.Substring(1), NumberStyles.Integer,
                                    NumberFormatInfo.InvariantInfo, out parsedValue);
                            }

                            if (parsedSuccessfully) {
                                parsedSuccessfully = (0 < parsedValue && parsedValue <= CfgConstants.UNICODE_00_END);
                            }

                            if (parsedSuccessfully) {
                                if (parsedValue <= CfgConstants.UNICODE_00_END) {
                                    // single character
                                    builder.Append((char)parsedValue);
                                } else {
                                    // multi-character
                                    var utf32 = (int)(parsedValue - CfgConstants.UNICODE_01_START);
                                    var leadingSurrogate = (char)((utf32 / 0x400) + CfgConstants.HIGH_SURROGATE);
                                    var trailingSurrogate = (char)((utf32 % 0x400) + CfgConstants.LOW_SURROGATE);

                                    builder.Append(leadingSurrogate);
                                    builder.Append(trailingSurrogate);
                                }

                                i = index;
                                continue;
                            }
                        } else {
                            i = index;
                            char entityChar;
                            CfgConstants.Entities.TryGetValue(entity, out entityChar);

                            if (entityChar != (char)0) {
                                c = entityChar;
                            } else {
                                builder.Append(CfgConstants.ENTITY_START);
                                builder.Append(entity);
                                builder.Append(CfgConstants.ENTITY_END);
                                continue;
                            }
                        }
                    }
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        public string Serialize() {
            var builder = new StringBuilder();
            var meta = GetMetadata(_type, Events);
            builder.AppendLine("<cfg-net>");
            foreach (var pair in meta) {
                builder.Append("<");
                builder.Append(pair.Key);
                builder.AppendLine(">");

                builder.Append("</");
                builder.Append(pair.Key);
                builder.AppendLine(">");
            }
            builder.Append("</cfg-net>");
            return builder.ToString();
        }

    }
}