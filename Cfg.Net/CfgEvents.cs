using System;
using System.Linq;
using Transformalize.Libs.Cfg.Net.Loggers;

namespace Transformalize.Libs.Cfg.Net {

    sealed class CfgEvents {
        private readonly CfgLogger _logger;

        public CfgEvents(CfgLogger logger) {
            _logger = logger;
        }

        public void DuplicateSet(string uniqueAttribute, object value, string nodeName) {
            _logger.Error(CfgConstants.PROBLEM_DUPLICATE_SET, uniqueAttribute, value, nodeName);
        }

        public void InvalidAttribute(string parentName, string nodeName, string attributeName, string validateAttributes) {
            _logger.Error(CfgConstants.PROBLEM_INVALID_ATTRIBUTE, parentName, nodeName, attributeName, Suffix(parentName), validateAttributes);
        }

        public void InvalidElement(string nodeName, string subNodeName) {
            _logger.Error(CfgConstants.PROBLEM_INVALID_ELEMENT, nodeName, subNodeName, Suffix(nodeName));
        }

        public void InvalidNestedElement(string parentName, string nodeName, string subNodeName) {
            _logger.Error(CfgConstants.PROBLEM_INVALID_NESTED_ELEMENT, parentName, nodeName, subNodeName, Suffix(parentName));
        }

        public void MissingAttribute(string parentName, string nodeName, string attributeName) {
            _logger.Error(CfgConstants.PROBLEM_MISSING_ATTRIBUTE, parentName, nodeName, attributeName, Suffix(parentName));
        }

        public void MissingElement(string nodeName, string elementName) {
            _logger.Error(CfgConstants.PROBLEM_MISSING_ELEMENT, nodeName == string.Empty ? "root" : "'" + nodeName + "'", elementName, Suffix(elementName));
        }

        public void MissingAddElement(string elementName) {
            _logger.Error(CfgConstants.PROBLEM_MISSING_ADD_ELEMENT, elementName, Suffix(elementName));
        }

        public void MissingNestedElement(string parentName, string nodeName, string elementName) {
            _logger.Error(CfgConstants.PROBLEM_MISSING_NESTED_ELEMENT, parentName, nodeName, elementName, Suffix(parentName), Suffix(elementName));
        }

        public void MissingPlaceHolderValues(string[] keys) {
            var formatted = "@(" + string.Join("), @(", keys) + ")";
            _logger.Error(CfgConstants.PROBLEM_MISSING_PLACE_HOLDER_VALUE, keys.Length == 1 ? "a value" : "values", formatted);
        }

        public void SettingValue(string propertyName, object value, string parentName, string nodeName, string message) {
            _logger.Error(CfgConstants.PROBLEM_SETTING_VALUE, propertyName, value, parentName, nodeName, message);
        }

        public void UnexpectedElement(string elementName, string subNodeName) {
            _logger.Error(CfgConstants.PROBLEM_UNEXPECTED_ELEMENT, subNodeName, elementName);
        }

        public void ValueNotInDomain(string parentName, string nodeName, string propertyName, object value, string validValues) {
            _logger.Error(CfgConstants.PROBLEM_VALUE_NOT_IN_DOMAIN, parentName, nodeName, propertyName, value, validValues, Suffix(parentName));
        }

        public void RootValueNotInDomain(object value, string propertyName, string validValues) {
            _logger.Error(CfgConstants.PROBLEM_ROOT_VALUE_NOT_IN_DOMAIN, value, propertyName, validValues);
        }

        public void SharedPropertyMissing(string name, string sharedProperty, string listType) {
            var type = listType.IndexOf('.') > 0 ? listType.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last() : listType;
            _logger.Error(CfgConstants.PROBLEM_SHARED_PROPERTY_MISSING, name, sharedProperty, type, Suffix(name));
        }

        public void ParseException(string message) {
            _logger.Error(CfgConstants.PROBLEM_PARSE, message);
        }

        private static string Suffix(string thing) {
            return thing == null || IsVowel(thing[0]) ? "n" : string.Empty;
        }

        [Obsolete("AddCustomProblem is deprecated, please use Error or Warning.")]
        public void AddCustomProblem(string problem, params object[] args) {
            _logger.Error(problem, args);
        }

        public void Error(string problem, params object[] args) {
            _logger.Error(problem, args);
        }

        public void Warning(string problem, params object[] args) {
            _logger.Warn(problem, args);
        }
        private static bool IsVowel(char c) {
            return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'A' || c == 'E' || c == 'I' ||
                   c == 'O' || c == 'U';
        }

        public void OnlyOneAttributeAllowed(string parentName, string name, int count) {
            _logger.Error(CfgConstants.PROBLEM_ONLY_ONE_ATTRIBUTE_ALLOWED, parentName, name, count);
        }

        public void TypeMismatch(string key, object value, Type propertyType) {
            _logger.Error(CfgConstants.PROBLEM_TYPE_MISMATCH, key, value, propertyType);
        }

        public void ValueTooShort(string name, string value, int minLength) {
            _logger.Error(CfgConstants.PROBLEM_VALUE_TOO_SHORT, name, value, minLength, value.Length);
        }

        public void ValueTooLong(string name, string value, int maxLength) {
            _logger.Error(CfgConstants.PROBLEM_VALUE_TOO_LONG, name, value, maxLength, value.Length);
        }

        public void ValueIsNotComparable(string name, object value) {
            _logger.Error(CfgConstants.PROBLEM_VALUE_IS_NOT_COMPARABLE, name, value);
        }

        public void ValueTooSmall(string name, object value, object minValue) {
            _logger.Error(CfgConstants.PROBLEM_VALUE_IS_TOO_SMALL, name, value, minValue);
        }

        public void ValueTooBig(string name, object value, object maxValue) {
            _logger.Error(CfgConstants.PROBLEM_VALUE_IS_TOO_BIG, name, value, maxValue);
        }

        public string[] Errors() {
            return _logger.Errors();
        }

        public string[] Warnings() {
            return _logger.Warnings();
        }

    }
}