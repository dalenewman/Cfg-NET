using System;
using System.Linq;
using System.Text;

namespace Transformalize.Libs.Cfg.Net {

    public class CfgProblems {

        private readonly StringBuilder _storage = new StringBuilder();

        public void DuplicateSet(string uniqueAttribute, object value, string nodeName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_DUPLICATE_SET, uniqueAttribute, value, nodeName);
            _storage.AppendLine();
        }

        public void InvalidAttribute(string parentName, string nodeName, string attributeName, string validateAttributes) {
            _storage.AppendFormat(CfgConstants.PROBLEM_INVALID_ATTRIBUTE, parentName, nodeName, attributeName, Suffix(parentName), validateAttributes);
            _storage.AppendLine();
        }

        public void InvalidElement(string nodeName, string subNodeName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_INVALID_ELEMENT, nodeName, subNodeName, Suffix(nodeName));
            _storage.AppendLine();
        }

        public void InvalidNestedElement(string parentName, string nodeName, string subNodeName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_INVALID_NESTED_ELEMENT, parentName, nodeName, subNodeName, Suffix(parentName));
            _storage.AppendLine();
        }

        public void MissingAttribute(string parentName, string nodeName, string attributeName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_MISSING_ATTRIBUTE, parentName, nodeName, attributeName, Suffix(parentName));
            _storage.AppendLine();
        }

        public void MissingElement(string nodeName, string elementName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_MISSING_ELEMENT, nodeName == string.Empty ? "root" : "'" + nodeName + "'", elementName, Suffix(elementName));
            _storage.AppendLine();
        }

        public void MissingAddElement(string elementName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_MISSING_ADD_ELEMENT, elementName, Suffix(elementName));
            _storage.AppendLine();
        }

        public void MissingNestedElement(string parentName, string nodeName, string elementName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_MISSING_NESTED_ELEMENT, parentName, nodeName, elementName, Suffix(parentName), Suffix(elementName));
            _storage.AppendLine();
        }

        public void MissingPlaceHolderValues(string[] keys) {
            var formatted = "@(" + string.Join("), @(", keys) + ")";
            _storage.AppendFormat(CfgConstants.PROBLEM_MISSING_PLACE_HOLDER_VALUE, keys.Length == 1 ? "a value" : "values", formatted);
            _storage.AppendLine();
        }

        public void SettingValue(string propertyName, object value, string parentName, string nodeName, string message) {
            _storage.AppendFormat(CfgConstants.PROBLEM_SETTING_VALUE, propertyName, value, parentName, nodeName, message);
            _storage.AppendLine();
        }

        public void UnexpectedElement(string elementName, string subNodeName) {
            _storage.AppendFormat(CfgConstants.PROBLEM_UNEXPECTED_ELEMENT, subNodeName, elementName);
            _storage.AppendLine();
        }

        public void ValueNotInDomain(string parentName, string nodeName, string propertyName, object value, string validValues) {
            _storage.AppendFormat(CfgConstants.PROBLEM_VALUE_NOT_IN_DOMAIN, parentName, nodeName, propertyName, value, validValues, Suffix(parentName));
            _storage.AppendLine();
        }

        public void RootValueNotInDomain(object value, string propertyName, string validValues) {
            _storage.AppendFormat(CfgConstants.PROBLEM_ROOT_VALUE_NOT_IN_DOMAIN, value, propertyName, validValues);
            _storage.AppendLine();
        }

        public void SharedPropertyMissing(string name, string sharedProperty, string listType) {
            var type = listType.IndexOf('.') > 0 ? listType.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last() : listType;
            _storage.AppendFormat(CfgConstants.PROBLEM_SHARED_PROPERTY_MISSING, name, sharedProperty, type, Suffix(name));
            _storage.AppendLine();
        }

        public void ParseException(string message) {
            _storage.AppendFormat(CfgConstants.PROBLEM_PARSE, message);
            _storage.AppendLine();
        }

        private static string Suffix(string thing) {
            return thing == null || IsVowel(thing[0]) ? "n" : string.Empty;
        }

        public string[] Yield() {
            return _storage.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void AddCustomProblem(string problem, params object[] args) {
            _storage.AppendFormat(problem, args);
            _storage.AppendLine();
        }

        private static bool IsVowel(char c) {
            return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'A' || c == 'E' || c == 'I' ||
                   c == 'O' || c == 'U';
        }

        public void OnlyOneAttributeAllowed(string parentName, string name, int count) {
            _storage.AppendFormat(CfgConstants.PROBLEM_ONLY_ONE_ATTRIBUTE_ALLOWED, parentName, name, count);
            _storage.AppendLine();
        }

        public void TypeMismatch(string key, object value, Type propertyType) {
            _storage.AppendFormat(CfgConstants.PROBLEM_TYPE_MISMATCH, key, value, propertyType);
            _storage.AppendLine();
        }

        public void ValueTooShort(string name, string value, int minLength) {
            _storage.AppendFormat(CfgConstants.PROBLEM_VALUE_TOO_SHORT, name, value, minLength, value.Length);
            _storage.AppendLine();
        }

        public void ValueTooLong(string name, string value, int maxLength) {
            _storage.AppendFormat(CfgConstants.PROBLEM_VALUE_TOO_LONG, name, value, maxLength, value.Length);
            _storage.AppendLine();
        }

        public void ValueIsNotComparable(string name, object value) {
            _storage.AppendFormat(CfgConstants.PROBLEM_VALUE_IS_NOT_COMPARABLE, name, value);
            _storage.AppendLine();
        }

        public void ValueTooSmall(string name, object value, object minValue) {
            _storage.AppendFormat(CfgConstants.PROBLEM_VALUE_IS_TOO_SMALL, name, value, minValue);
            _storage.AppendLine();
        }

        public void ValueTooBig(string name, object value, object maxValue) {
            _storage.AppendFormat(CfgConstants.PROBLEM_VALUE_IS_TOO_BIG, name, value, maxValue);
            _storage.AppendLine();
        }

    }
}