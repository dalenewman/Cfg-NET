using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Transformalize.Libs.Cfg.Net.Loggers;
using Transformalize.Libs.Cfg.Net.Parsers;
using Transformalize.Libs.Cfg.Net.Shorthand;

namespace Transformalize.Libs.Cfg.Net {

   public abstract class CfgNode {

      internal static string ControlString = ((char)31).ToString();
      internal static char NamedParameterSplitter = ':';
      readonly IParser _parser;
      readonly ILogger _logger;
      ShorthandRoot _shorthand;

      //shared cache
      static bool _initialized;
      static readonly object Locker = new object();
      static Dictionary<Type, Dictionary<string, CfgMetadata>> _metadataCache;
      static Dictionary<Type, List<string>> _propertyCache;
      static Dictionary<Type, List<string>> _elementCache;
      static Dictionary<Type, Dictionary<string, string>> _nameCache;
      static Dictionary<Type, Func<string, object>> _converter;

      readonly Dictionary<string, string> _uniqueProperties = new Dictionary<string, string>();
      readonly StringBuilder _builder = new StringBuilder();
      readonly Type _type;
      Dictionary<string, CfgMetadata> _metadata;
      CfgEvents _events;
      static Dictionary<string, char> _entities;

      protected CfgNode(IParser parser = null, ILogger logger = null) {
         _parser = parser;
         _logger = logger;
         _type = GetType();

         lock (Locker) {
            if (_initialized)
               return;
            Initialize();
            _initialized = true;
         }

      }

      static void Initialize() {
         _metadataCache = new Dictionary<Type, Dictionary<string, CfgMetadata>>();
         _propertyCache = new Dictionary<Type, List<string>>();
         _elementCache = new Dictionary<Type, List<string>>();
         _nameCache = new Dictionary<Type, Dictionary<string, string>>();
         _converter = new Dictionary<Type, Func<string, object>> {
                {typeof (String), (x => x)},
                {typeof (Guid), (x => Guid.Parse(x))},
                {typeof (Int16), (x => Convert.ToInt16(x))},
                {typeof (Int32), (x => Convert.ToInt32(x))},
                {typeof (Int64), (x => Convert.ToInt64(x))},
                {typeof (UInt16), (x => Convert.ToUInt16(x))},
                {typeof (UInt32), (x => Convert.ToUInt32(x))},
                {typeof (UInt64), (x => Convert.ToUInt64(x))},
                {typeof (Double), (x => Convert.ToDouble(x))},
                {typeof (Decimal), (x => Decimal.Parse(x, NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol, (IFormatProvider) CultureInfo.CurrentCulture.GetFormat(typeof (NumberFormatInfo)))) },
                {typeof (Char), (x => Convert.ToChar(x))},
                {typeof (DateTime), (x => Convert.ToDateTime(x))},
                {typeof (Boolean), (x => Convert.ToBoolean(x))},
                {typeof (Single), (x => Convert.ToSingle(x))},
                {typeof (Byte), (x => Convert.ToByte(x))}
            };
      }

      /// <summary>
      /// Get any type that inherits from CfgNode with default values
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="setter"></param>
      /// <returns></returns>
      public T GetDefaultOf<T>(Action<T> setter = null) {
         var obj = Activator.CreateInstance(typeof(T));

         SetDefaults(obj, GetMetadata(typeof(T), _events, _builder));

         if (setter != null) {
            setter((T)obj);
         }

          ((CfgNode)obj).Modify();

         return (T)obj;
      }

      static void SetDefaults(object node, Dictionary<string, CfgMetadata> metadata) {
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

      [Obsolete("AddProblem is deprecated, please use Error or Warn instead.")]
      protected void AddProblem(string problem, params object[] args) {
         _events.AddCustomProblem(problem, args);
      }

      protected void Error(string message, params object[] args) {
         _events.Error(message, args);
      }

      protected void Warn(string message, params object[] args) {
         _events.Warning(message, args);
      }

      protected void LoadShorthand(string cfg) {

         _events = _events ?? new CfgEvents(new CfgLogger(new MemoryLogger(), _logger));

         _shorthand = new ShorthandRoot(cfg);

         if (_shorthand.Warnings().Any()) {
            foreach (var warning in _shorthand.Warnings()) {
               _events.Warning(warning);
            }
         }

         if (_shorthand.Errors().Any()) {
            foreach (var error in _shorthand.Errors()) {
               _events.Error(error);
            }
            return;
         }

         _shorthand.InitializeMethodDataLookup();
      }

      public void Load(string cfg, Dictionary<string, string> parameters = null) {

         _events = _events ?? new CfgEvents(new CfgLogger(new MemoryLogger(), _logger));

         _metadata = GetMetadata(_type, _events, _builder);
         SetDefaults(this, _metadata);

         INode node;
         try {
            cfg = cfg.Trim();
            var parser = _parser ?? (cfg.StartsWith("{") ? (IParser)new FastJsonParser() : (IParser)new NanoXmlParser());
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
            _events.ParseException(ex.Message);
            return;
         }

         LoadProperties(node, null, parameters);
         LoadCollections(node, null, parameters);
         Modify();
         Validate();
      }

      protected IEnumerable<string[]> LoadEnvironment(INode node, Dictionary<string, string> parameters) {

         for (var i = 0; i < node.SubNodes.Count; i++) {
            var environmentsNode = node.SubNodes[i];
            if (environmentsNode.Name != CfgConstants.ENVIRONMENTS_ELEMENT_NAME)
               continue;

            if (environmentsNode.SubNodes.Count == 0)
               break;

            INode environmentNode;

            if (environmentsNode.SubNodes.Count > 1) {
               IAttribute defaultEnvironment;
               if (!environmentsNode.TryAttribute(CfgConstants.ENVIRONMENTS_DEFAULT_NAME, out defaultEnvironment))
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

            if (parametersNode.Name != CfgConstants.PARAMETERS_ELEMENT_NAME || environmentNode.SubNodes.Count == 0)
               break;

            return GetParameters(parametersNode);

         }

         return Enumerable.Empty<string[]>();
      }

      static IEnumerable<string[]> GetParameters(INode parametersNode) {
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

      CfgNode Load(INode node, string parentName, CfgEvents events, ShorthandRoot shorthand, Dictionary<string, string> parameters) {
         _events = events;
         _shorthand = shorthand;
         _metadata = GetMetadata(_type, events, _builder);
         SetDefaults(this, _metadata);
         LoadProperties(node, parentName, parameters);
         LoadCollections(node, parentName, parameters);
         Modify();
         Validate();
         return this;
      }

      /// <summary>
      /// Override to add custom validation.  Use `AddProblem()` to add problems.
      /// </summary>
      protected virtual void Validate() { }

      /// <summary>
      /// Override for custom modifications.
      /// </summary>
      protected virtual void Modify() { }

      void LoadCollections(INode node, string parentName, Dictionary<string, string> parameters = null) {

         var keys = _elementCache[_type];
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
            var subNodeKey = NormalizeName(_type, subNode.Name, _builder);
            if (_metadata.ContainsKey(subNodeKey)) {
               elementHits.Add(subNodeKey);
               var item = _metadata[subNodeKey];

               object value = null;
               CfgMetadata sharedCfg = null;

               if (item.SharedProperty != null) {
                  var sharedMetadata = GetMetadata(item.ListType, _events, _builder);
                  if (sharedMetadata.ContainsKey(item.SharedProperty)) {
                     sharedCfg = sharedMetadata[item.SharedProperty];
                  } else {
                     _events.SharedPropertyMissing(subNode.Name, item.SharedProperty, item.ListType.ToString());
                  }
                  IAttribute sharedAttribute;
                  if (subNode.TryAttribute(item.SharedProperty, out sharedAttribute)) {
                     value = sharedAttribute.Value ?? item.SharedValue;
                  }
               }

               for (var j = 0; j < subNode.SubNodes.Count; j++) {
                  var add = subNode.SubNodes[j];
                  if (add.Name.Equals("add", StringComparison.Ordinal)) {
                     var addKey = NormalizeName(_type, subNode.Name, _builder);
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
                              var attrValue = add.Attributes[0].Value;
                              if (item.ListType == typeof(string) || item.ListType == typeof(object)) {
                                 elements[addKey].Add(attrValue);
                              } else {
                                 try {
                                    elements[addKey].Add(_converter[item.ListType](attrValue));
                                 } catch (Exception ex) {
                                    _events.SettingValue(subNode.Name, attrValue, parentName, subNode.Name, ex.Message);
                                 }
                              }
                           } else {
                              _events.OnlyOneAttributeAllowed(parentName, subNode.Name, add.Attributes.Count);
                           }
                        }
                     } else {
                        var loaded = item.Loader().Load(add, subNode.Name, _events, _shorthand, parameters);
                        if (sharedCfg != null) {
                           var sharedValue = sharedCfg.Getter(loaded);
                           if (sharedValue == null) {
                              sharedCfg.Setter(loaded, value ?? item.SharedValue);
                           }
                        }
                        elements[addKey].Add(loaded);
                     }
                  } else {
                     _events.UnexpectedElement(add.Name, subNode.Name);
                  }
               }
            } else {
               if (parentName == null) {
                  _events.InvalidElement(node.Name, subNode.Name);
               } else {
                  _events.InvalidNestedElement(parentName, node.Name, subNode.Name);
               }
            }
         }

         // check for duplicates of unique properties required to be unique in collections
         for (var i = 0; i < keys.Count; i++) {
            var key = keys[i];
            var item = _metadata[key];
            var list = elements[key];

            if (list.Count > 1) {

               lock (Locker) {
                  if (item.UniquePropertiesInList == null) {
                     item.UniquePropertiesInList = GetMetadata(item.ListType, _events, _builder)
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
                     _events.DuplicateSet(unique, duplicates[l], key);
                  }
               }
            } else if (list.Count == 0 && item.Attribute.required) {
               if (elementHits.Contains(key) && !addHits.Contains(key)) {
                  _events.MissingAddElement(key);
               } else {
                  if (parentName == null) {
                     _events.MissingElement(node.Name, key);
                  } else {
                     _events.MissingNestedElement(parentName, node.Name, key);
                  }
               }
            }

         }
      }

      static string NormalizeName(Type type, string name, StringBuilder builder) {
         var cache = _nameCache[type];
         if (cache.ContainsKey(name)) {
            return cache[name];
         }
         builder.Clear();
         for (var i = 0; i < name.Length; i++) {
            var character = name[i];
            if (char.IsLetterOrDigit(character)) {
               builder.Append(char.IsUpper(character) ? char.ToLowerInvariant(character) : character);
            }
         }
         var result = builder.ToString();
         cache[name] = result;
         return result;
      }

      void LoadProperties(INode node, string parentName, IDictionary<string, string> parameters = null) {

         var keys = _propertyCache[_type];

         if (keys.Count == 0)
            return;

         var keyHits = new HashSet<string>();

         for (var i = 0; i < node.Attributes.Count; i++) {

            var attribute = node.Attributes[i];
            var attributeKey = NormalizeName(_type, attribute.Name, _builder);
            if (_metadata.ContainsKey(attributeKey)) {

               if (attribute.Value == null)
                  continue;

               var decoded = false;
               attribute.Value = CheckParameters(parameters, attribute.Value);

               if (attribute.Value.IndexOf(CfgConstants.ENTITY_START) > -1) {
                  attribute.Value = Decode(attribute.Value, _builder);
                  decoded = true;
               }

               var item = _metadata[attributeKey];

               if (item.Attribute.toLower) {
                  attribute.Value = attribute.Value.ToLower();
               } else if (item.Attribute.toUpper) {
                  attribute.Value = attribute.Value.ToUpper();
               }

               if (item.Attribute.unique) {
                  UniqueProperties[attributeKey] = attribute.Value;
               }

               if (item.Attribute.shorthand) {
                  if (_shorthand == null || _shorthand.MethodDataLookup == null) {
                     _events.ShorthandNotLoaded(parentName, node.Name, attribute.Name);
                  } else {
                     TranslateShorthand(node, attribute);
                  }
               }

               if (item.PropertyInfo.PropertyType == typeof(string) || item.PropertyInfo.PropertyType == typeof(object)) {
                  item.Setter(this, attribute.Value);
                  keyHits.Add(attributeKey);
               } else {
                  try {
                     item.Setter(this, _converter[item.PropertyInfo.PropertyType](attribute.Value));
                     keyHits.Add(attributeKey);
                  } catch (Exception ex) {
                     _events.SettingValue(attribute.Name, attribute.Value, parentName, node.Name, ex.Message);
                  }
               }

               // Setter has been called and may have changed the value
               var value = item.Getter(this);

               if (item.Attribute.NeedString) {

                  var stringValue = item.PropertyInfo.PropertyType == typeof(string) ? (string)value : value.ToString();

                  if (item.Attribute.DomainSet) {
                     if (!item.IsInDomain(stringValue)) {
                        if (parentName == null) {
                           _events.RootValueNotInDomain(value, attribute.Name, item.Attribute.domain.Replace(item.Attribute.domainDelimiter.ToString(), ", "));
                        } else {
                           _events.ValueNotInDomain(parentName, node.Name, attribute.Name, value, item.Attribute.domain.Replace(item.Attribute.domainDelimiter.ToString(), ", "));
                        }
                     }
                  }

                  CheckValueLength(item.Attribute, attribute.Name, stringValue);

               }

               CheckValueBoundaries(item.Attribute, attribute.Name, value);

               attribute.Value = decoded ? Encode(value.ToString(), _builder) : value.ToString();
            } else {
               _events.InvalidAttribute(parentName, node.Name, attribute.Name, string.Join(", ", keys));
            }
         }

         // missing any required attributes?
         foreach (var key in keys.Except(keyHits)) {
            var item = _metadata[key];
            if (item.Attribute.required) {
               _events.MissingAttribute(parentName, node.Name, key);
            }
         }

      }

      void TranslateShorthand(INode node, IAttribute attribute) {

         var expressions = new Expressions(attribute.Value);
         var shorthandNodes = new Dictionary<string, List<INode>>();

         for (var j = 0; j < expressions.Count; j++) {
            var expression = expressions[j];
            if (_shorthand.MethodDataLookup.ContainsKey(expression.Method)) {
               var methodData = _shorthand.MethodDataLookup[expression.Method];
               var shorthandNode = new ShorthandNode("add");
               shorthandNode.Attributes.Add(new ShorthandAttribute(methodData.Target.Property, expression.Method));

               var signatureParameters = methodData.Signature.Parameters.Select(p => new Parameter { Name = p.Name, Value = p.Value }).ToList();
               var passedParameters = expression.Parameters.Select(p => new string(p.ToCharArray())).ToArray();

               // single parameters
               if (methodData.Signature.Parameters.Count == 1 && expression.SingleParameter != string.Empty) {
                  var name = methodData.Signature.Parameters[0].Name;
                  var value = expression.SingleParameter.StartsWith(name + ":", StringComparison.OrdinalIgnoreCase) ? expression.SingleParameter.Remove(0, name.Length + 1) : expression.SingleParameter;
                  shorthandNode.Attributes.Add(new ShorthandAttribute(name, value));
               } else {
                  // named parameters
                  for (int i = 0; i < passedParameters.Length; i++) {
                     var parameter = passedParameters[i];
                     var split = Split(parameter, NamedParameterSplitter);
                     if (split.Length == 2) {
                        var name = NormalizeName(typeof(Parameter), split[0], _builder);
                        shorthandNode.Attributes.Add(new ShorthandAttribute(name, split[1]));
                        signatureParameters.RemoveAll(p => NormalizeName(typeof(Parameter), p.Name, _builder) == name);
                        expression.Parameters.RemoveAll(p => p == parameter);
                     }
                  }

                  // ordered nameless parameters
                  for (int m = 0; m < signatureParameters.Count; m++) {
                     var signatureParameter = signatureParameters[m];
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
            var shorthandCollection = node.SubNodes.FirstOrDefault(sn => sn.Name == pair.Key);
            if (shorthandCollection == null) {
               shorthandCollection = new ShorthandNode(pair.Key);
               shorthandCollection.SubNodes.AddRange(pair.Value);
               node.SubNodes.Add(shorthandCollection);
            } else {
               shorthandCollection.SubNodes.InsertRange(0, pair.Value);
            }
         }
      }

      void CheckValueLength(CfgAttribute itemAttributes, string name, string value) {

         if (itemAttributes.MinLengthSet) {
            if (value.Length < itemAttributes.minLength) {
               _events.ValueTooShort(name, value, itemAttributes.minLength);
            }
         }

         if (!itemAttributes.MaxLengthSet)
            return;

         if (value.Length > itemAttributes.maxLength) {
            _events.ValueTooLong(name, value, itemAttributes.maxLength);
         }
      }

      void CheckValueBoundaries(CfgAttribute itemAttributes, string name, object value) {

         if (!itemAttributes.MinValueSet && !itemAttributes.MaxValueSet)
            return;

         var comparable = value as IComparable;
         if (comparable == null) {
            _events.ValueIsNotComparable(name, value);
         } else {
            if (itemAttributes.MinValueSet) {
               if (comparable.CompareTo(itemAttributes.minValue) < 0) {
                  _events.ValueTooSmall(name, value, itemAttributes.minValue);
               }
            }

            if (!itemAttributes.MaxValueSet)
               return;

            if (comparable.CompareTo(itemAttributes.maxValue) > 0) {
               _events.ValueTooBig(name, value, itemAttributes.maxValue);
            }
         }
      }

      string CheckParameters(IDictionary<string, string> parameters, string input) {
         if (parameters == null || input.IndexOf('@') < 0)
            return input;
         var response = ReplaceParameters(input, parameters, _builder);
         if (response.Item2.Length > 1) {
            _events.MissingPlaceHolderValues(response.Item2);
         }
         return response.Item1;
      }

      static Tuple<string, string[]> ReplaceParameters(string value, IDictionary<string, string> parameters, StringBuilder builder) {
         builder.Clear();
         List<string> badKeys = null;
         for (var j = 0; j < value.Length; j++) {
            if (value[j] == CfgConstants.PLACE_HOLDER_FIRST &&
                value.Length > j + 1 &&
                value[j + 1] == CfgConstants.PLACE_HOLDER_SECOND) {
               var length = 2;
               while (value.Length > j + length && value[j + length] != CfgConstants.PLACE_HOLDER_LAST) {
                  length++;
               }
               if (length > 2) {
                  var key = value.Substring(j + 2, length - 2);
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

      protected Dictionary<string, string> UniqueProperties {
         get { return _uniqueProperties; }
      }

      static Dictionary<string, char> Entities {
         get {
            return _entities ?? (_entities = new Dictionary<string, char>(StringComparer.Ordinal)
            {
                    {"Aacute", "\x00c1"[0]},
                    {"aacute", "\x00e1"[0]},
                    {"Acirc", "\x00c2"[0]},
                    {"acirc", "\x00e2"[0]},
                    {"acute", "\x00b4"[0]},
                    {"AElig", "\x00c6"[0]},
                    {"aelig", "\x00e6"[0]},
                    {"Agrave", "\x00c0"[0]},
                    {"agrave", "\x00e0"[0]},
                    {"alefsym", "\x2135"[0]},
                    {"Alpha", "\x0391"[0]},
                    {"alpha", "\x03b1"[0]},
                    {"amp", "\x0026"[0]},
                    {"and", "\x2227"[0]},
                    {"ang", "\x2220"[0]},
                    {"apos", "\x0027"[0]},
                    {"Aring", "\x00c5"[0]},
                    {"aring", "\x00e5"[0]},
                    {"asymp", "\x2248"[0]},
                    {"Atilde", "\x00c3"[0]},
                    {"atilde", "\x00e3"[0]},
                    {"Auml", "\x00c4"[0]},
                    {"auml", "\x00e4"[0]},
                    {"bdquo", "\x201e"[0]},
                    {"Beta", "\x0392"[0]},
                    {"beta", "\x03b2"[0]},
                    {"brvbar", "\x00a6"[0]},
                    {"bull", "\x2022"[0]},
                    {"cap", "\x2229"[0]},
                    {"Ccedil", "\x00c7"[0]},
                    {"ccedil", "\x00e7"[0]},
                    {"cedil", "\x00b8"[0]},
                    {"cent", "\x00a2"[0]},
                    {"Chi", "\x03a7"[0]},
                    {"chi", "\x03c7"[0]},
                    {"circ", "\x02c6"[0]},
                    {"clubs", "\x2663"[0]},
                    {"cong", "\x2245"[0]},
                    {"copy", "\x00a9"[0]},
                    {"crarr", "\x21b5"[0]},
                    {"cup", "\x222a"[0]},
                    {"curren", "\x00a4"[0]},
                    {"dagger", "\x2020"[0]},
                    {"Dagger", "\x2021"[0]},
                    {"darr", "\x2193"[0]},
                    {"dArr", "\x21d3"[0]},
                    {"deg", "\x00b0"[0]},
                    {"Delta", "\x0394"[0]},
                    {"delta", "\x03b4"[0]},
                    {"diams", "\x2666"[0]},
                    {"divide", "\x00f7"[0]},
                    {"Eacute", "\x00c9"[0]},
                    {"eacute", "\x00e9"[0]},
                    {"Ecirc", "\x00ca"[0]},
                    {"ecirc", "\x00ea"[0]},
                    {"Egrave", "\x00c8"[0]},
                    {"egrave", "\x00e8"[0]},
                    {"empty", "\x2205"[0]},
                    {"emsp", "\x2003"[0]},
                    {"ensp", "\x2002"[0]},
                    {"Epsilon", "\x0395"[0]},
                    {"epsilon", "\x03b5"[0]},
                    {"equiv", "\x2261"[0]},
                    {"Eta", "\x0397"[0]},
                    {"eta", "\x03b7"[0]},
                    {"ETH", "\x00d0"[0]},
                    {"eth", "\x00f0"[0]},
                    {"Euml", "\x00cb"[0]},
                    {"euml", "\x00eb"[0]},
                    {"euro", "\x20ac"[0]},
                    {"exist", "\x2203"[0]},
                    {"fnof", "\x0192"[0]},
                    {"forall", "\x2200"[0]},
                    {"frac12", "\x00bd"[0]},
                    {"frac14", "\x00bc"[0]},
                    {"frac34", "\x00be"[0]},
                    {"frasl", "\x2044"[0]},
                    {"Gamma", "\x0393"[0]},
                    {"gamma", "\x03b3"[0]},
                    {"ge", "\x2265"[0]},
                    {"gt", "\x003e"[0]},
                    {"harr", "\x2194"[0]},
                    {"hArr", "\x21d4"[0]},
                    {"hearts", "\x2665"[0]},
                    {"hellip", "\x2026"[0]},
                    {"Iacute", "\x00cd"[0]},
                    {"iacute", "\x00ed"[0]},
                    {"Icirc", "\x00ce"[0]},
                    {"icirc", "\x00ee"[0]},
                    {"iexcl", "\x00a1"[0]},
                    {"Igrave", "\x00cc"[0]},
                    {"igrave", "\x00ec"[0]},
                    {"image", "\x2111"[0]},
                    {"infin", "\x221e"[0]},
                    {"int", "\x222b"[0]},
                    {"Iota", "\x0399"[0]},
                    {"iota", "\x03b9"[0]},
                    {"iquest", "\x00bf"[0]},
                    {"isin", "\x2208"[0]},
                    {"Iuml", "\x00cf"[0]},
                    {"iuml", "\x00ef"[0]},
                    {"Kappa", "\x039a"[0]},
                    {"kappa", "\x03ba"[0]},
                    {"Lambda", "\x039b"[0]},
                    {"lambda", "\x03bb"[0]},
                    {"lang", "\x2329"[0]},
                    {"laquo", "\x00ab"[0]},
                    {"larr", "\x2190"[0]},
                    {"lArr", "\x21d0"[0]},
                    {"lceil", "\x2308"[0]},
                    {"ldquo", "\x201c"[0]},
                    {"le", "\x2264"[0]},
                    {"lfloor", "\x230a"[0]},
                    {"lowast", "\x2217"[0]},
                    {"loz", "\x25ca"[0]},
                    {"lrm", "\x200e"[0]},
                    {"lsaquo", "\x2039"[0]},
                    {"lsquo", "\x2018"[0]},
                    {"lt", "\x003c"[0]},
                    {"macr", "\x00af"[0]},
                    {"mdash", "\x2014"[0]},
                    {"micro", "\x00b5"[0]},
                    {"middot", "\x00b7"[0]},
                    {"minus", "\x2212"[0]},
                    {"Mu", "\x039c"[0]},
                    {"mu", "\x03bc"[0]},
                    {"nabla", "\x2207"[0]},
                    {"nbsp", "\x00a0"[0]},
                    {"ndash", "\x2013"[0]},
                    {"ne", "\x2260"[0]},
                    {"ni", "\x220b"[0]},
                    {"not", "\x00ac"[0]},
                    {"notin", "\x2209"[0]},
                    {"nsub", "\x2284"[0]},
                    {"Ntilde", "\x00d1"[0]},
                    {"ntilde", "\x00f1"[0]},
                    {"Nu", "\x039d"[0]},
                    {"nu", "\x03bd"[0]},
                    {"Oacute", "\x00d3"[0]},
                    {"oacute", "\x00f3"[0]},
                    {"Ocirc", "\x00d4"[0]},
                    {"ocirc", "\x00f4"[0]},
                    {"OElig", "\x0152"[0]},
                    {"oelig", "\x0153"[0]},
                    {"Ograve", "\x00d2"[0]},
                    {"ograve", "\x00f2"[0]},
                    {"oline", "\x203e"[0]},
                    {"Omega", "\x03a9"[0]},
                    {"omega", "\x03c9"[0]},
                    {"Omicron", "\x039f"[0]},
                    {"omicron", "\x03bf"[0]},
                    {"oplus", "\x2295"[0]},
                    {"or", "\x2228"[0]},
                    {"ordf", "\x00aa"[0]},
                    {"ordm", "\x00ba"[0]},
                    {"Oslash", "\x00d8"[0]},
                    {"oslash", "\x00f8"[0]},
                    {"Otilde", "\x00d5"[0]},
                    {"otilde", "\x00f5"[0]},
                    {"otimes", "\x2297"[0]},
                    {"Ouml", "\x00d6"[0]},
                    {"ouml", "\x00f6"[0]},
                    {"para", "\x00b6"[0]},
                    {"part", "\x2202"[0]},
                    {"permil", "\x2030"[0]},
                    {"perp", "\x22a5"[0]},
                    {"Phi", "\x03a6"[0]},
                    {"phi", "\x03c6"[0]},
                    {"Pi", "\x03a0"[0]},
                    {"pi", "\x03c0"[0]},
                    {"piv", "\x03d6"[0]},
                    {"plusmn", "\x00b1"[0]},
                    {"pound", "\x00a3"[0]},
                    {"prime", "\x2032"[0]},
                    {"Prime", "\x2033"[0]},
                    {"prod", "\x220f"[0]},
                    {"prop", "\x221d"[0]},
                    {"Psi", "\x03a8"[0]},
                    {"psi", "\x03c8"[0]},
                    {"quot", "\x0022"[0]},
                    {"radic", "\x221a"[0]},
                    {"rang", "\x232a"[0]},
                    {"raquo", "\x00bb"[0]},
                    {"rarr", "\x2192"[0]},
                    {"rArr", "\x21d2"[0]},
                    {"rceil", "\x2309"[0]},
                    {"rdquo", "\x201d"[0]},
                    {"real", "\x211c"[0]},
                    {"reg", "\x00ae"[0]},
                    {"rfloor", "\x230b"[0]},
                    {"Rho", "\x03a1"[0]},
                    {"rho", "\x03c1"[0]},
                    {"rlm", "\x200f"[0]},
                    {"rsaquo", "\x203a"[0]},
                    {"rsquo", "\x2019"[0]},
                    {"sbquo", "\x201a"[0]},
                    {"Scaron", "\x0160"[0]},
                    {"scaron", "\x0161"[0]},
                    {"sdot", "\x22c5"[0]},
                    {"sect", "\x00a7"[0]},
                    {"shy", "\x00ad"[0]},
                    {"Sigma", "\x03a3"[0]},
                    {"sigma", "\x03c3"[0]},
                    {"sigmaf", "\x03c2"[0]},
                    {"sim", "\x223c"[0]},
                    {"spades", "\x2660"[0]},
                    {"sub", "\x2282"[0]},
                    {"sube", "\x2286"[0]},
                    {"sum", "\x2211"[0]},
                    {"sup", "\x2283"[0]},
                    {"sup1", "\x00b9"[0]},
                    {"sup2", "\x00b2"[0]},
                    {"sup3", "\x00b3"[0]},
                    {"supe", "\x2287"[0]},
                    {"szlig", "\x00df"[0]},
                    {"Tau", "\x03a4"[0]},
                    {"tau", "\x03c4"[0]},
                    {"there4", "\x2234"[0]},
                    {"Theta", "\x0398"[0]},
                    {"theta", "\x03b8"[0]},
                    {"thetasym", "\x03d1"[0]},
                    {"thinsp", "\x2009"[0]},
                    {"THORN", "\x00de"[0]},
                    {"thorn", "\x00fe"[0]},
                    {"tilde", "\x02dc"[0]},
                    {"times", "\x00d7"[0]},
                    {"trade", "\x2122"[0]},
                    {"Uacute", "\x00da"[0]},
                    {"uacute", "\x00fa"[0]},
                    {"uarr", "\x2191"[0]},
                    {"uArr", "\x21d1"[0]},
                    {"Ucirc", "\x00db"[0]},
                    {"ucirc", "\x00fb"[0]},
                    {"Ugrave", "\x00d9"[0]},
                    {"ugrave", "\x00f9"[0]},
                    {"uml", "\x00a8"[0]},
                    {"upsih", "\x03d2"[0]},
                    {"Upsilon", "\x03a5"[0]},
                    {"upsilon", "\x03c5"[0]},
                    {"Uuml", "\x00dc"[0]},
                    {"uuml", "\x00fc"[0]},
                    {"weierp", "\x2118"[0]},
                    {"Xi", "\x039e"[0]},
                    {"xi", "\x03be"[0]},
                    {"Yacute", "\x00dd"[0]},
                    {"yacute", "\x00fd"[0]},
                    {"yen", "\x00a5"[0]},
                    {"yuml", "\x00ff"[0]},
                    {"Yuml", "\x0178"[0]},
                    {"Zeta", "\x0396"[0]},
                    {"zeta", "\x03b6"[0]},
                    {"zwj", "\x200d"[0]},
                    {"zwnj", "\x200c"[0]}
                });
         }
      }

      [Obsolete("Problems method is obsolete. Use Errors(), Warnings(), or Logs()")]
      public List<string> Problems() {
         return Errors().ToList();
      }

      public List<string> Logs() {
         var list = new List<string>(Errors());
         list.AddRange(Warnings());
         return list;
      }

      public string[] Errors() {
         return _events.Errors();
      }

      public string[] Warnings() {
         return _events.Warnings();
      }

      static Dictionary<string, CfgMetadata> GetMetadata(Type type, CfgEvents events, StringBuilder sb) {

         Dictionary<string, CfgMetadata> metadata;

         if (_metadataCache.TryGetValue(type, out metadata))
            return metadata;

         lock (Locker) {

            _nameCache[type] = new Dictionary<string, string>();

            var keyCache = new List<string>();
            var listCache = new List<string>();
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            metadata = new Dictionary<string, CfgMetadata>(StringComparer.Ordinal);
            for (var i = 0; i < propertyInfos.Length; i++) {
               var propertyInfo = propertyInfos[i];

               if (!propertyInfo.CanRead)
                  continue;
               if (!propertyInfo.CanWrite)
                  continue;
               var attribute = (CfgAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(CfgAttribute), true);
               if (attribute == null)
                  continue;

               var key = NormalizeName(type, propertyInfo.Name, sb);
               var item = new CfgMetadata(propertyInfo, attribute);

               // check default value for type mismatch
               if (attribute.ValueIsSet) {
                  if (attribute.value.GetType() != propertyInfo.PropertyType) {
                     var value = attribute.value;
                     if (TryConvertValue(ref value, propertyInfo.PropertyType)) {
                        attribute.value = value;
                     } else {
                        item.TypeMismatch = true;
                        events.TypeMismatch(key, value, propertyInfo.PropertyType);
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
                  if (attribute.sharedProperty != null) {
                     item.SharedProperty = attribute.sharedProperty;
                     item.SharedValue = attribute.sharedValue;
                  }
               } else {
                  keyCache.Add(key);
               }
               item.Setter = ReflectionHelper.CreateSetter(propertyInfo);
               item.Getter = ReflectionHelper.CreateGetter(propertyInfo);

               metadata[key] = item;
            }

            _propertyCache[type] = keyCache;
            _elementCache[type] = listCache;
            _metadataCache[type] = metadata;

         }

         return _metadataCache[type];

      }

      static bool ResolveType(Func<bool> isSet, ref object input, string key, CfgMetadata metadata, CfgEvents events) {
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

         var split = arg.Replace("\\" + splitter, ControlString).Split(splitter);
         return split.Select(s => s.Replace(ControlString, "\\" + splitter)).Skip(skip).Where(s => !string.IsNullOrEmpty(s)).ToArray();
      }

      internal static string[] Split(string arg, string[] splitter, int skip = 0) {
         if (arg.Equals(string.Empty))
            return new string[0];

         var split = arg.Replace("\\" + splitter[0], ControlString).Split(splitter, StringSplitOptions.None);
         return split.Select(s => s.Replace(ControlString, splitter[0])).Skip(skip).Where(s => !string.IsNullOrEmpty(s)).ToArray();
      }

      // a naive implementation for hand-written configurations
      static string Encode(string value, StringBuilder builder) {

         builder.Clear();
         for (var i = 0; i < value.Length; i++) {
            var ch = value[0];
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

      public static string Decode(string input, StringBuilder builder) {

         builder.Clear();
         var htmlEntityEndingChars = new[] { CfgConstants.ENTITY_END, CfgConstants.ENTITY_START };

         for (var i = 0; i < input.Length; i++) {
            var c = input[i];

            if (c == CfgConstants.ENTITY_START) {
               // Found &. Look for the next ; or &. If & occurs before ;, then this is not entity, and next & may start another entity
               var index = input.IndexOfAny(htmlEntityEndingChars, i + 1);
               if (index > 0 && input[index] == CfgConstants.ENTITY_END) {
                  var entity = input.Substring(i + 1, index - i - 1);

                  if (entity.Length > 1 && entity[0] == '#') {

                     bool parsedSuccessfully;
                     uint parsedValue;
                     if (entity[1] == 'x' || entity[1] == 'X') {
                        parsedSuccessfully = UInt32.TryParse(entity.Substring(2), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out parsedValue);
                     } else {
                        parsedSuccessfully = UInt32.TryParse(entity.Substring(1), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out parsedValue);
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
                     Entities.TryGetValue(entity, out entityChar);

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

   }

}
