﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace Transformalize.Libs.Cfg.Net.fastJSON
{
    public delegate string Serialize(object data);
    public delegate object Deserialize(string data);

    public sealed class JSONParameters
    {
        /// <summary>
        /// Use the optimized fast Dataset Schema format (default = True)
        /// </summary>
        public bool UseOptimizedDatasetSchema = true;
        /// <summary>
        /// Use the fast GUID format (default = True)
        /// </summary>
        public bool UseFastGuid = true;
        /// <summary>
        /// Serialize null values to the output (default = True)
        /// </summary>
        public bool SerializeNullValues = true;
        /// <summary>
        /// Use the UTC date format (default = True)
        /// </summary>
        public bool UseUTCDateTime = true;
        /// <summary>
        /// Show the readonly properties of types in the output (default = False)
        /// </summary>
        public bool ShowReadOnlyProperties = false;
        /// <summary>
        /// Use the $types extension to optimise the output json (default = True)
        /// </summary>
        public bool UsingGlobalTypes = true;
        /// <summary>
        /// Ignore case when processing json and deserializing 
        /// </summary>
        [Obsolete("Not needed anymore and will always match")]
        public bool IgnoreCaseOnDeserialize = false;
        /// <summary>
        /// Anonymous types have read only properties 
        /// </summary>
        public bool EnableAnonymousTypes = false;
        /// <summary>
        /// Enable fastJSON extensions $types, $type, $map (default = True)
        /// </summary>
        public bool UseExtensions = true;
        /// <summary>
        /// Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
        /// </summary>
        public bool UseEscapedUnicode = true;
        /// <summary>
        /// Output string key dictionaries as "k"/"v" format (default = False) 
        /// </summary>
        public bool KVStyleStringDictionary = false;
        /// <summary>
        /// Output Enum values instead of names (default = False)
        /// </summary>
        public bool UseValuesOfEnums = false;
        /// <summary>
        /// Ignore attributes to check for (default : XmlIgnoreAttribute)
        /// </summary>
        public List<Type> IgnoreAttributes = new List<Type>();
        /// <summary>
        /// If you have parametric and no default constructor for you classes (default = False)
        /// 
        /// IMPORTANT NOTE : If True then all initial values within the class will be ignored and will be not set
        /// </summary>
        public bool ParametricConstructorOverride = false;
        /// <summary>
        /// Serialize DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
        /// </summary>
        public bool DateTimeMilliseconds = false;
        /// <summary>
        /// Maximum depth for circular references in inline mode (default = 20)
        /// </summary>
        public byte SerializerMaxDepth = 20;
        /// <summary>
        /// Inline circular or already seen objects instead of replacement with $i (default = False) 
        /// </summary>
        public bool InlineCircularReferences = false;
        /// <summary>
        /// Save property/field names as lowercase (default = false)
        /// </summary>
        public bool SerializeToLowerCaseNames = false;

        public void FixValues()
        {
            if (UseExtensions == false) // disable conflicting params
            {
                UsingGlobalTypes = false;
                InlineCircularReferences = true;
            }
            if (EnableAnonymousTypes)
                ShowReadOnlyProperties = true;
        }
    }

    public static class JSON
    {
        /// <summary>
        /// Globally set-able parameters for controlling the serializer
        /// </summary>
        public static JSONParameters Parameters = new JSONParameters();
        /// <summary>
        /// Create a formatted json string (beautified) from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ToNiceJSON(object obj, JSONParameters param)
        {
            string s = ToJSON(obj, param);

            return Beautify(s);
        }
        /// <summary>
        /// Create a json representation for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJSON(object obj)
        {
            return ToJSON(obj, JSON.Parameters);
        }
        /// <summary>
        /// Create a json representation for an object with parameter override on this call
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ToJSON(object obj, JSONParameters param)
        {
            param.FixValues();
            Type t = null;

            if (obj == null)
                return "null";

            if (obj.GetType().IsGenericType)
                t = Reflection.Instance.GetGenericTypeDefinition(obj.GetType());
            if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                param.UsingGlobalTypes = false;

            // FEATURE : enable extensions when you can deserialize anon types
            if (param.EnableAnonymousTypes) { param.UseExtensions = false; param.UsingGlobalTypes = false; }
            return new JSONSerializer(param).ConvertToJSON(obj);
        }
        /// <summary>
        /// Parse a json string and generate a Dictionary&lt;string,object&gt; or List&lt;object&gt; structure
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Parse(string json)
        {
            return new JsonParser(json).Decode();
        }
        /// <summary>
        /// Create a typed generic object from the json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json)
        {
            return new deserializer(Parameters).ToObject<T>(json);
        }
        /// <summary>
        /// Create a typed generic object from the json with parameter override on this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json, JSONParameters param)
        {
            return new deserializer(param).ToObject<T>(json);
        }
        /// <summary>
        /// Create an object from the json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object ToObject(string json)
        {
            return new deserializer(Parameters).ToObject(json, null);
        }
        /// <summary>
        /// Create an object from the json with parameter override on this call
        /// </summary>
        /// <param name="json"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object ToObject(string json, JSONParameters param)
        {
            return new deserializer(param).ToObject(json, null);
        }
        /// <summary>
        /// Create an object of type from the json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToObject(string json, Type type)
        {
            return new deserializer(Parameters).ToObject(json, type);
        }
        /// <summary>
        /// Fill a given object with the json represenation
        /// </summary>
        /// <param name="input"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object FillObject(object input, string json)
        {
            Dictionary<string, object> ht = new JsonParser(json).Decode() as Dictionary<string, object>;
            if (ht == null) return null;
            return new deserializer(Parameters).ParseDictionary(ht, null, input.GetType(), input);
        }
        /// <summary>
        /// Deep copy an object i.e. clone to a new object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DeepCopy(object obj)
        {
            return new deserializer(Parameters).ToObject(ToJSON(obj));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T obj)
        {
            return new deserializer(Parameters).ToObject<T>(ToJSON(obj));
        }

        /// <summary>
        /// Create a human readable string from the json 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Beautify(string input)
        {
            return Formatter.PrettyPrint(input);
        }
        /// <summary>
        /// Register custom type handlers for your own types not natively handled by fastJSON
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serializer"></param>
        /// <param name="deserializer"></param>
        public static void RegisterCustomType(Type type, Serialize serializer, Deserialize deserializer)
        {
            Reflection.Instance.RegisterCustomType(type, serializer, deserializer);
        }
        /// <summary>
        /// Clear the internal reflection cache so you can start from new (you will loose performance)
        /// </summary>
        public static void ClearReflectionCache()
        {
            Reflection.Instance.ClearReflectionCache();
        }

        internal static long CreateLong(out long num, string s, int index, int count)
        {
            num = 0;
            bool neg = false;
            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }
            if (neg) num = -num;

            return num;
        }
    }

    internal class deserializer
    {
        public deserializer(JSONParameters param)
        {
            _params = param;
        }

        private JSONParameters _params;
        private bool _usingglobals = false;
        private Dictionary<object, int> _circobj = new Dictionary<object, int>();
        private Dictionary<int, object> _cirrev = new Dictionary<int, object>();

        public T ToObject<T>(string json)
        {
            Type t = typeof(T);
            var o = ToObject(json, t);

            if (t.IsArray)
            {
                if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
                {
                    Type tt = t.GetElementType();
                    object oo = Array.CreateInstance(tt, 0);
                    return (T)oo;
                }
                else
                    return (T)o;
            }
            else
                return (T)o;
        }

        public object ToObject(string json)
        {
            return ToObject(json, null);
        }

        public object ToObject(string json, Type type)
        {
            //_params = Parameters;
            _params.FixValues();
            Type t = null;
            if (type != null && type.IsGenericType)
                t = Reflection.Instance.GetGenericTypeDefinition(type);
            if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                _params.UsingGlobalTypes = false;
            _usingglobals = _params.UsingGlobalTypes;

            object o = new JsonParser(json).Decode();
            if (o == null)
                return null;

            if (o is IDictionary)
            {
                if (type != null && t == typeof(Dictionary<,>)) // deserialize a dictionary
                    return RootDictionary(o, type);
                else // deserialize an object
                    return ParseDictionary(o as Dictionary<string, object>, null, type, null);
            }

            if (o is List<object>)
            {
                if (type != null && t == typeof(Dictionary<,>)) // kv format
                    return RootDictionary(o, type);

                if (type != null && t == typeof(List<>)) // deserialize to generic list
                    return RootList(o, type);

                if (type == typeof(Hashtable))
                    return RootHashTable((List<object>)o);
                else
                    return (o as List<object>).ToArray();
            }

            if (type != null && o.GetType() != type)
                return ChangeType(o, type);

            return o;
        }

        #region [   p r i v a t e   m e t h o d s   ]
        private object RootHashTable(List<object> o)
        {
            Hashtable h = new Hashtable();

            foreach (Dictionary<string, object> values in o)
            {
                object key = values["k"];
                object val = values["v"];
                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, null, typeof(object), null);

                if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, null, typeof(object), null);

                h.Add(key, val);
            }

            return h;
        }

        private object ChangeType(object value, Type conversionType)
        {
            if (conversionType == typeof(int))
                return (int)((long)value);

            else if (conversionType == typeof(long))
                return (long)value;

            else if (conversionType == typeof(string))
                return (string)value;

            else if (conversionType.IsEnum)
                return CreateEnum(conversionType, value);

            else if (conversionType == typeof(DateTime))
                return CreateDateTime((string)value);

            else if (Reflection.Instance.IsTypeRegistered(conversionType))
                return Reflection.Instance.CreateCustom((string)value, conversionType);

            // 8-30-2014 - James Brooks - Added code for nullable types.
            if (IsNullable(conversionType))
            {
                if (value == null)
                {
                    return value;
                }
                conversionType = UnderlyingTypeOf(conversionType);
            }

            // 8-30-2014 - James Brooks - Nullable Guid is a special case so it was moved after the "IsNullable" check.
            if (conversionType == typeof(Guid))
                return CreateGuid((string)value);

            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }

        private bool IsNullable(Type t)
        {
            if (!t.IsGenericType) return false;
            Type g = t.GetGenericTypeDefinition();
            return (g.Equals(typeof(Nullable<>)));
        }

        private Type UnderlyingTypeOf(Type t)
        {
            return t.GetGenericArguments()[0];
        }

        private object RootList(object parse, Type type)
        {
            Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
            IList o = (IList)Reflection.Instance.FastCreateInstance(type);
            foreach (var k in (IList)parse)
            {
                _usingglobals = false;
                object v = k;
                if (k is Dictionary<string, object>)
                    v = ParseDictionary(k as Dictionary<string, object>, null, gtypes[0], null);
                else
                    v = ChangeType(k, gtypes[0]);

                o.Add(v);
            }
            return o;
        }

        private object RootDictionary(object parse, Type type)
        {
            Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
            Type t1 = null;
            Type t2 = null;
            if (gtypes != null)
            {
                t1 = gtypes[0];
                t2 = gtypes[1];
            }
            if (parse is Dictionary<string, object>)
            {
                IDictionary o = (IDictionary)Reflection.Instance.FastCreateInstance(type);

                foreach (var kv in (Dictionary<string, object>)parse)
                {
                    object v;
                    object k = ChangeType(kv.Key, t1);

                    if (kv.Value is Dictionary<string, object>)
                        v = ParseDictionary(kv.Value as Dictionary<string, object>, null, t2, null);

                    else if (t2.IsArray)
                        v = CreateArray((List<object>)kv.Value, t2, t2.GetElementType(), null);

                    else if (kv.Value is IList)
                        v = CreateGenericList((List<object>)kv.Value, t2, t1, null);

                    else
                        v = ChangeType(kv.Value, t2);

                    o.Add(k, v);
                }

                return o;
            }
            if (parse is List<object>)
                return CreateDictionary(parse as List<object>, type, gtypes, null);

            return null;
        }

        internal object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object> globaltypes, Type type, object input)
        {
            object tn = "";
            if (type == typeof(NameValueCollection))
                return CreateNV(d);
            if (type == typeof(StringDictionary))
                return CreateSD(d);

            if (d.TryGetValue("$i", out tn))
            {
                object v = null;
                _cirrev.TryGetValue((int)(long)tn, out v);
                return v;
            }

            if (d.TryGetValue("$types", out tn))
            {
                _usingglobals = true;
                globaltypes = new Dictionary<string, object>();
                foreach (var kv in (Dictionary<string, object>)tn)
                {
                    globaltypes.Add((string)kv.Value, kv.Key);
                }
            }

            bool found = d.TryGetValue("$type", out tn);
#if !SILVERLIGHT
            if (found == false && type == typeof(System.Object))
            {
                return d;   // CreateDataset(d, globaltypes);
            }
#endif
            if (found)
            {
                if (_usingglobals)
                {
                    object tname = "";
                    if (globaltypes != null && globaltypes.TryGetValue((string)tn, out tname))
                        tn = tname;
                }
                type = Reflection.Instance.GetTypeFromCache((string)tn);
            }

            if (type == null)
                throw new Exception("Cannot determine type");

            string typename = type.FullName;
            object o = input;
            if (o == null)
            {
                if (_params.ParametricConstructorOverride)
                    o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                else
                    o = Reflection.Instance.FastCreateInstance(type);
            }
            int circount = 0;
            if (_circobj.TryGetValue(o, out circount) == false)
            {
                circount = _circobj.Count + 1;
                _circobj.Add(o, circount);
                _cirrev.Add(circount, o);
            }

            Dictionary<string, myPropInfo> props = Reflection.Instance.Getproperties(type, typename, Reflection.Instance.IsTypeRegistered(type));
            foreach (string n in d.Keys)
            {
                string name = n.ToLower();
                if (name == "$map")
                {
                    ProcessMap(o, props, (Dictionary<string, object>)d[name]);
                    continue;
                }
                myPropInfo pi;
                if (props.TryGetValue(name, out pi) == false)
                    continue;
                if (pi.CanWrite)
                {
                    object v = d[n];

                    if (v != null)
                    {
                        object oset = null;

                        switch (pi.Type)
                        {
                            case myPropInfoType.Int: oset = (int)((long)v); break;
                            case myPropInfoType.Long: oset = (long)v; break;
                            case myPropInfoType.String: oset = (string)v; break;
                            case myPropInfoType.Bool: oset = (bool)v; break;
                            case myPropInfoType.DateTime: oset = CreateDateTime((string)v); break;
                            case myPropInfoType.Enum: oset = CreateEnum(pi.pt, v); break;
                            case myPropInfoType.Guid: oset = CreateGuid((string)v); break;

                            case myPropInfoType.Array:
                                if (!pi.IsValueType)
                                    oset = CreateArray((List<object>)v, pi.pt, pi.bt, globaltypes);
                                // what about 'else'?
                                break;
                            case myPropInfoType.ByteArray: oset = Convert.FromBase64String((string)v); break;
                            case myPropInfoType.Dictionary: oset = CreateDictionary((List<object>)v, pi.pt, pi.GenericTypes, globaltypes); break;
                            case myPropInfoType.StringKeyDictionary: oset = CreateStringKeyDictionary((Dictionary<string, object>)v, pi.pt, pi.GenericTypes, globaltypes); break;
                            case myPropInfoType.NameValue: oset = CreateNV((Dictionary<string, object>)v); break;
                            case myPropInfoType.StringDictionary: oset = CreateSD((Dictionary<string, object>)v); break;
                            case myPropInfoType.Custom: oset = Reflection.Instance.CreateCustom((string)v, pi.pt); break;
                            default:
                                {
                                    if (pi.IsGenericType && pi.IsValueType == false && v is List<object>)
                                        oset = CreateGenericList((List<object>)v, pi.pt, pi.bt, globaltypes);

                                    else if ((pi.IsClass || pi.IsStruct) && v is Dictionary<string, object>)
                                        oset = ParseDictionary((Dictionary<string, object>)v, globaltypes, pi.pt, pi.getter(o));

                                    else if (v is List<object>)
                                        oset = CreateArray((List<object>)v, pi.pt, typeof(object), globaltypes);

                                    else if (pi.IsValueType)
                                        oset = ChangeType(v, pi.changeType);

                                    else
                                        oset = v;
                                }
                                break;
                        }

                        o = pi.setter(o, oset);
                    }
                }
            }
            return o;
        }

        private StringDictionary CreateSD(Dictionary<string, object> d)
        {
            StringDictionary nv = new StringDictionary();

            foreach (var o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        private NameValueCollection CreateNV(Dictionary<string, object> d)
        {
            NameValueCollection nv = new NameValueCollection();

            foreach (var o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        private void ProcessMap(object obj, Dictionary<string, myPropInfo> props, Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> kv in dic)
            {
                myPropInfo p = props[kv.Key];
                object o = p.getter(obj);
                Type t = Type.GetType((string)kv.Value);
                if (t == typeof(Guid))
                    p.setter(obj, CreateGuid((string)o));
            }
        }

        private int CreateInteger(string s, int index, int count)
        {
            int num = 0;
            bool neg = false;
            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }
            if (neg) num = -num;

            return num;
        }

        private object CreateEnum(Type pt, object v)
        {
            // TODO : optimize create enum
            return Enum.Parse(pt, v.ToString());
        }

        private Guid CreateGuid(string s)
        {
            if (s.Length > 30)
                return new Guid(s);
            else
                return new Guid(Convert.FromBase64String(s));
        }

        private DateTime CreateDateTime(string value)
        {
            bool utc = false;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  Z
            int year;
            int month;
            int day;
            int hour;
            int min;
            int sec;
            int ms = 0;

            year = CreateInteger(value, 0, 4);
            month = CreateInteger(value, 5, 2);
            day = CreateInteger(value, 8, 2);
            hour = CreateInteger(value, 11, 2);
            min = CreateInteger(value, 14, 2);
            sec = CreateInteger(value, 17, 2);
            if (value.Length > 21 && value[19] == '.')
                ms = CreateInteger(value, 20, 3);

            if (value[value.Length - 1] == 'Z')
                utc = true;

            if (_params.UseUTCDateTime == false && utc == false)
                return new DateTime(year, month, day, hour, min, sec, ms);
            else
                return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
        }

        private object CreateArray(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            Array col = Array.CreateInstance(bt, data.Count);
            // create an array of objects
            for (int i = 0; i < data.Count; i++)
            {
                object ob = data[i];
                if (ob is IDictionary)
                    col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null), i);
                else
                    col.SetValue(ChangeType(ob, bt), i);
            }

            return col;
        }


        private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            IList col = (IList)Reflection.Instance.FastCreateInstance(pt);
            // create an array of objects
            foreach (object ob in data)
            {
                if (ob is IDictionary)
                    col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null));

                else if (ob is List<object>)
                {
                    if (bt.IsGenericType)
                        col.Add((List<object>)ob);//).ToArray());
                    else
                        col.Add(((List<object>)ob).ToArray());
                }
                else
                    col.Add(ChangeType(ob, bt));
            }
            return col;
        }

        private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            var col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (KeyValuePair<string, object> values in reader)
            {
                var key = values.Key;
                object val = null;

                if (values.Value is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null);

                else if (types != null && t2.IsArray)
                {
                    if (values.Value is Array)
                        val = values.Value;
                    else
                        val = CreateArray((List<object>)values.Value, t2, t2.GetElementType(), globalTypes);
                }
                else if (values.Value is IList)
                    val = CreateGenericList((List<object>)values.Value, t2, t1, globalTypes);

                else
                    val = ChangeType(values.Value, t2);

                col.Add(key, val);
            }

            return col;
        }

        private object CreateDictionary(List<object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            IDictionary col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (Dictionary<string, object> values in reader)
            {
                object key = values["k"];
                object val = values["v"];

                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
                else
                    key = ChangeType(key, t1);

                if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
                else
                    val = ChangeType(val, t2);

                col.Add(key, val);
            }

            return col;
        }

        #endregion
    }

}