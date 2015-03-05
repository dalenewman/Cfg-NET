namespace Transformalize.Libs.Cfg.Net
{
    public static class CfgConstants {

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
        public static string ENVIRONMENTS_DEFAULT_NAME = "default";
        public static string ENVIRONMENTS_ELEMENT_NAME = "environments";
        public static string PARAMETERS_ELEMENT_NAME = "parameters";

        //PROBLEM PATTERNS
        public static string PROBLEM_DUPLICATE_SET = "You set a duplicate '{0}' value '{1}' in '{2}'.";
        public static string PROBLEM_INVALID_ATTRIBUTE = "A{3} '{0}' '{1}' element contains an invalid '{2}' attribute.  Valid attributes are: {4}.";
        public static string PROBLEM_INVALID_ELEMENT = "A{2} '{0}' element has an invalid '{1}' element.  If you need a{2} '{1}' element, decorate it with the Cfg[()] attribute in your Cfg-NET model.";
        public static string PROBLEM_INVALID_NESTED_ELEMENT = "A{3} '{0}' '{1}' element has an invalid '{2}' element.";
        public static string PROBLEM_MISSING_ADD_ELEMENT = "A{1} '{0}' element is missing a child element.";
        public static string PROBLEM_MISSING_ATTRIBUTE = "A{3} '{0}' '{1}' element is missing a '{2}' attribute.";
        public static string PROBLEM_MISSING_ELEMENT = "The {0} element is missing a{2} '{1}' element.";
        public static string PROBLEM_MISSING_NESTED_ELEMENT = "A{3} '{0}' '{1}' element is missing a{4} '{2}' element.";
        public static string PROBLEM_MISSING_PLACE_HOLDER_VALUE = "You're missing {0} for {1}.";
        public static string PROBLEM_SETTING_VALUE = "Could not set '{0}' to '{1}' inside '{2}' '{3}'. {4}";
        public static string PROBLEM_UNEXPECTED_ELEMENT = "Invalid element {0} in {1}.  Only 'add' elements are allowed here.";
        public static string PROBLEM_PARSE = "Could not parse the configuration. {0}";
        public static string PROBLEM_VALUE_NOT_IN_DOMAIN = "A{5} '{0}' '{1}' element has an invalid value of '{3}' in the '{2}' attribute.  The valid domain is: {4}.";
        public static string PROBLEM_ROOT_VALUE_NOT_IN_DOMAIN = "The root element has an invalid value of '{0}' in the '{1}' attribute.  The valid domain is: {2}.";
        public static string PROBLEM_SHARED_PROPERTY_MISSING = "A{3} '{0}' shared property '{1}' is missing in '{2}'.  Make sure it is defined and decorated with [Cfg()].";
        public static string PROBLEM_ONLY_ONE_ATTRIBUTE_ALLOWED = "You must have exactly 1 attribute in '{0}' '{1}'.  You have {2}.";
        public static string PROBLEM_TYPE_MISMATCH = "The `{0}` attribute's default value's type ({1}) does not match the property type ({2}).";
        public static string PROBLEM_VALUE_TOO_SHORT = "The `{0}` attribute's value `{1}` is too short. It's {3} characters. It must be at least {2} characters.";
        public static string PROBLEM_VALUE_TOO_LONG = "The `{0}` attribute's value `{1}` is too long. It's {3} characters. It must not exceed {2} characters.";
        // ReSharper restore InconsistentNaming
    }
}