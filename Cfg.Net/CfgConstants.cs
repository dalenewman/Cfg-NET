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
using System.Collections.Generic;

namespace Cfg.Net {

    internal static class CfgConstants {

        private static Dictionary<string, char> _entities;

        // ReSharper disable InconsistentNaming
        public static char ENTITY_END = ';';
        public static char ENTITY_START = '&';
        public static char HIGH_SURROGATE = '\uD800';
        public static char LOW_SURROGATE = '\uDC00';
        public static char PLACE_HOLDER_FIRST = '@';
        public static char PLACE_HOLDER_LAST = ')';
        public static char PLACE_HOLDER_SECOND = '(';
        public static int UNICODE_00_END = 0x00FFFF;
        public static int UNICODE_01_START = 0x10000;
        public static string ENVIRONMENTS_DEFAULT_NAME = "environment";
        public static string ENVIRONMENTS_ELEMENT_NAME = "environments";
        public static string PARAMETERS_ELEMENT_NAME = "parameters";

        //PROBLEM PATTERNS
        public static string PROBLEM_DUPLICATE_SET = "Duplicate '{0}' value '{1}' in '{2}'.";

        public static string PROBLEM_INVALID_ATTRIBUTE =
            "A{3} '{0}' '{1}' element contains an invalid '{2}' attribute.  Valid attributes are: {4}.";

        public static string PROBLEM_INVALID_ELEMENT =
            "A{2} '{0}' element has an invalid '{1}' element.  If you need a{2} '{1}' element, decorate it with the Cfg[()] attribute in your Cfg-NET model.";

        public static string PROBLEM_INVALID_NESTED_ELEMENT = "A{3} '{0}' '{1}' element has an invalid '{2}' element.";
        public static string PROBLEM_MISSING_ADD_ELEMENT = "A{1} '{0}' element is missing a child element.";

        public static string PROBLEM_MISSING_VALIDATOR =
            "A '{1}' attribute in {0} can not find the {2} validator.  Please make it's passed in to your Cfg-Net root constructor.";

        public static string PROBLEM_VALIDATOR_EXCEPTION =
            "The '{0}' validator threw an exception when validating the value '{2}'. {1}";

        public static string PROBLEM_MISSING_ATTRIBUTE = "A{3} '{0}' '{1}' element is missing a '{2}' attribute.";

        public static string PROBLEM_SHORTHAND_NOT_LOADED =
            "A{3} '{0}' '{1}' element's '{2}' attribute needs a shorthand configuration, but none was loaded.";

        public static string PROBLEM_MISSING_ELEMENT = "The {0} element is missing a{2} '{1}' element.";
        public static string PROBLEM_MISSING_NESTED_ELEMENT = "A{3} '{0}' '{1}' element is missing a{4} '{2}' element.";
        public static string PROBLEM_MISSING_PLACE_HOLDER_VALUE = "Missing {0} for {1}.";
        public static string PROBLEM_SETTING_VALUE = "Could not set '{0}' to '{1}' inside '{2}' '{3}'. {4}";

        public static string PROBLEM_UNEXPECTED_ELEMENT =
            "Invalid element {0} in {1}.  Only 'add' elements are allowed here.";

        public static string PROBLEM_PARSE = "Could not parse the configuration. {0}";

        public static string PROBLEM_VALUE_NOT_IN_DOMAIN =
            "A{4} '{0}' element has an invalid value of '{2}' in the '{1}' attribute.  The valid domain is: {3}.";

        public static string PROBLEM_ROOT_VALUE_NOT_IN_DOMAIN =
            "The root element has an invalid value of '{0}' in the '{1}' attribute.  The valid domain is: {2}.";

        public static string PROBLEM_SHARED_PROPERTY_MISSING =
            "A{3} '{0}' shared property '{1}' is missing in '{2}'.  Make sure it is defined and decorated with [Cfg()].";

        public static string PROBLEM_ONLY_ONE_ATTRIBUTE_ALLOWED =
            "The '{0}' '{1}' collection does not inherit from CfgNode, so you can only have one value (attribute).  You have {2} defined. So, limit it to one, or have your list item inherit from CfgNode.";

        public static string PROBLEM_TYPE_MISMATCH =
            "The '{0}' attribute default value '{1}' does not have the same type as the property type of '{2}'.";

        public static string PROBLEM_VALUE_TOO_SHORT =
            "The '{0}' attribute value '{1}' is too short. It is {3} characters. It must be at least {2} characters.";

        public static string PROBLEM_VALUE_TOO_LONG =
            "The '{0}' attribute value '{1}' is too long. It is {3} characters. It must not exceed {2} characters.";

        public static string PROBLEM_VALUE_IS_NOT_COMPARABLE =
            "The '{0}' attribute value '{1}' is not comparable.  Having a minValue or maxValue set on an incomparable property type causes this.";

        public static string PROBLEM_VALUE_IS_TOO_SMALL =
            "The '{0}' attribute value '{1}' is too small. The minimum value allowed is '{2}'.";

        public static string PROBLEM_VALUE_IS_TOO_BIG =
            "The '{0}' attribute value '{1}' is too big. The maximum value allowed is '{2}'.";

        // ReSharper restore InconsistentNaming


        internal static Dictionary<string, char> Entities => _entities ?? (_entities = new Dictionary<string, char>(StringComparer.Ordinal) {
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