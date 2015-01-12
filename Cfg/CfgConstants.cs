namespace Transformalize.Libs.Cfg.Net {
    public static class CfgConstants {

        // ReSharper disable InconsistentNaming
        public static string ATTRIBUTE_NAME = "name";
        public static char ENTITY_END = ';';
        public static char ENTITY_START = '&';
        public static char HIGH_SURROGATE = '\uD800';
        public static char LOW_SURROGATE = '\uDC00';
        public static char PLACE_HOLDER_FIRST = '@';
        public static char PLACE_HOLDER_LAST = ')';
        public static char PLACE_HOLDER_SECOND = '(';
        public static int UNICODE_00_END = 0x00FFFF;
        public static int UNICODE_01_START = 0x10000;
        public static string ENVIRONMENTS_DEFAULT_NAME = "default";
        public static string ENVIRONMENTS_ELEMENT_NAME = "environments";
        public static string PARAMETERS_ELEMENT_NAME = "parameters";

        //PROBLEM PATTERNS
        public static string PROBLEM_DUPLICATE = "There is a duplicate {0} value {1} in {2}.";
        public static string PROBLEM_DUPLICATE_SET = "You set a duplicate '{0}' value '{1}' in '{2}'.";
        public static string PROBLEM_INVALID_ATTRIBUTE = "A{3} '{0}' '{1}' element contains an invalid '{2}' attribute.  Valid attributes are: {4}.";
        public static string PROBLEM_INVALID_ELEMENT = "A{2} '{0}' element has an invalid '{1}' element.";
        public static string PROBLEM_INVALID_NESTED_ELEMENT = "A{3} '{0}' '{1}' element has an invalid '{2}' element.";
        public static string PROBLEM_MISSING_ADD_ELEMENT = "A{1} '{0}' element is missing an 'add' element.";
        public static string PROBLEM_MISSING_ATTRIBUTE = "A{3} '{0}' '{1}' element is missing a '{2}' attribute.";
        public static string PROBLEM_MISSING_ELEMENT = "The '{0}' element is missing a{2} '{1}' element.";
        public static string PROBLEM_MISSING_NESTED_ELEMENT = "A{3} '{0}' '{1}' element is missing a{4} '{2}' element.";
        public static string PROBLEM_MISSING_PLACE_HOLDER_VALUE = "You're missing a value for @({0})";
        public static string PROBLEM_SETTING_PROPERTY = "Could not set property {0} to value {1} from attribute {2}. {3}";
        public static string PROBLEM_SETTING_VALUE = "Could not set '{0}' to '{1}' inside '{2}' '{3}'. {4}";
        public static string PROBLEM_UNEXPECTED_ELEMENT = "Invalid element {0} in {1}.  Only 'add' elements are allowed here.";
        public static string PROBLEM_XML_PARSE = "Could not parse the configuration. {0}";
        // ReSharper restore InconsistentNaming

    }
}