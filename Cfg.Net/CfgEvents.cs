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
using System.Linq;
using Cfg.Net.Loggers;

namespace Cfg.Net {
    internal sealed class CfgEvents {

        public CfgLogger Logger { get; set; }

        public CfgEvents(CfgLogger logger) {
            Logger = logger;
        }

        public void DuplicateSet(string uniqueAttribute, object value, string nodeName) {
            Logger.Error(CfgConstants.PROBLEM_DUPLICATE_SET, uniqueAttribute, value, nodeName);
        }

        public void InvalidAttribute(string parentName, string nodeName, string attributeName, string validateAttributes) {
            Logger.Error(CfgConstants.PROBLEM_INVALID_ATTRIBUTE, parentName, nodeName, attributeName,
                Suffix(parentName), validateAttributes);
        }

        public void InvalidElement(string nodeName, string subNodeName) {
            Logger.Error(CfgConstants.PROBLEM_INVALID_ELEMENT, nodeName, subNodeName, Suffix(nodeName));
        }

        public void InvalidNestedElement(string parentName, string nodeName, string subNodeName) {
            Logger.Error(CfgConstants.PROBLEM_INVALID_NESTED_ELEMENT, parentName, nodeName, subNodeName,
                Suffix(parentName));
        }

        public void MissingAttribute(string parentName, string nodeName, string attributeName) {
            Logger.Error(CfgConstants.PROBLEM_MISSING_ATTRIBUTE, parentName, nodeName, attributeName,
                Suffix(parentName));
        }

        public void MissingElement(string nodeName, string elementName) {
            Logger.Error(CfgConstants.PROBLEM_MISSING_ELEMENT, nodeName == string.Empty ? "root" : "'" + nodeName + "'",
                elementName, Suffix(elementName));
        }

        public void MissingAddElement(string elementName) {
            Logger.Error(CfgConstants.PROBLEM_MISSING_ADD_ELEMENT, elementName, Suffix(elementName));
        }

        public void MissingValidator(string parentName, string nodeName, string validatorName) {
            Logger.Warn(CfgConstants.PROBLEM_MISSING_VALIDATOR, parentName, nodeName, validatorName);
        }

        public void ValidatorException(string validatorName, Exception ex, object value) {
            Logger.Error(CfgConstants.PROBLEM_VALIDATOR_EXCEPTION, validatorName, ex.Message, value);
        }

        public void MissingNestedElement(string parentName, string nodeName, string elementName) {
            Logger.Error(CfgConstants.PROBLEM_MISSING_NESTED_ELEMENT, parentName, nodeName, elementName,
                Suffix(parentName), Suffix(elementName));
        }

        public void MissingPlaceHolderValues(string[] keys) {
            string formatted = "@(" + string.Join("), @(", keys) + ")";
            Logger.Error(CfgConstants.PROBLEM_MISSING_PLACE_HOLDER_VALUE, keys.Length == 1 ? "a value" : "values",
                formatted);
        }

        public void SettingValue(string propertyName, object value, string parentName, string nodeName, string message) {
            Logger.Error(CfgConstants.PROBLEM_SETTING_VALUE, propertyName, value, parentName, nodeName, message);
        }

        public void UnexpectedElement(string elementName, string subNodeName) {
            Logger.Error(CfgConstants.PROBLEM_UNEXPECTED_ELEMENT, subNodeName, elementName);
        }

        public void ShorthandNotLoaded(string parentName, string nodeName, string attributeName) {
            Logger.Error(CfgConstants.PROBLEM_SHORTHAND_NOT_LOADED, parentName, nodeName, attributeName,
                Suffix(parentName));
        }

        public void ValueNotInDomain(string parentName, string propertyName, object value, string validValues) {
            Logger.Error(CfgConstants.PROBLEM_VALUE_NOT_IN_DOMAIN, parentName, propertyName, value, validValues, Suffix(parentName));
        }

        public void RootValueNotInDomain(object value, string propertyName, string validValues) {
            Logger.Error(CfgConstants.PROBLEM_ROOT_VALUE_NOT_IN_DOMAIN, value, propertyName, validValues);
        }

        public void SharedPropertyMissing(string name, string sharedProperty, string listType) {
            var type = listType.IndexOf('.') > 0
                ? listType.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last()
                : listType;
            Logger.Error(CfgConstants.PROBLEM_SHARED_PROPERTY_MISSING, name, sharedProperty, type, Suffix(name));
        }

        public void ParseException(string message) {
            Logger.Error(CfgConstants.PROBLEM_PARSE, message);
        }

        private static string Suffix(string thing) {
            return thing == null || IsVowel(thing[0]) ? "n" : string.Empty;
        }

        public void Error(string problem, params object[] args) {
            Logger.Error(problem, args);
        }

        public void Warning(string problem, params object[] args) {
            Logger.Warn(problem, args);
        }

        private static bool IsVowel(char c) {
            return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'A' || c == 'E' || c == 'I' ||
                   c == 'O' || c == 'U';
        }

        public void OnlyOneAttributeAllowed(string parentName, string name, int count) {
            Logger.Error(CfgConstants.PROBLEM_ONLY_ONE_ATTRIBUTE_ALLOWED, parentName, name, count);
        }

        public void TypeMismatch(string key, object value, Type propertyType) {
            Logger.Error(CfgConstants.PROBLEM_TYPE_MISMATCH, key, value, propertyType);
        }

        public void ValueTooShort(string name, string value, int minLength) {
            Logger.Error(CfgConstants.PROBLEM_VALUE_TOO_SHORT, name, value, minLength, value.Length);
        }

        public void ValueTooLong(string name, string value, int maxLength) {
            Logger.Error(CfgConstants.PROBLEM_VALUE_TOO_LONG, name, value, maxLength, value.Length);
        }

        public void ValueIsNotComparable(string name, object value) {
            Logger.Error(CfgConstants.PROBLEM_VALUE_IS_NOT_COMPARABLE, name, value);
        }

        public void ValueTooSmall(string name, object value, object minValue) {
            Logger.Error(CfgConstants.PROBLEM_VALUE_IS_TOO_SMALL, name, value, minValue);
        }

        public void ValueTooBig(string name, object value, object maxValue) {
            Logger.Error(CfgConstants.PROBLEM_VALUE_IS_TOO_BIG, name, value, maxValue);
        }

        public string[] Errors() {
            return Logger.Errors();
        }

        public string[] Warnings() {
            return Logger.Warnings();
        }

    }
}