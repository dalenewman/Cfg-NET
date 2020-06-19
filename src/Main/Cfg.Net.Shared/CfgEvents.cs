// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2018 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using Cfg.Net.Loggers;

namespace Cfg.Net {

    internal sealed class CfgEvents {

        public DefaultLogger Logger { get; set; }

        public CfgEvents(DefaultLogger logger) {
            Logger = logger;
        }

        public void DuplicateSet(string uniqueAttribute, object value, string nodeName) {
            Logger.Error("Duplicate {0} value {1} in {2}.", uniqueAttribute, value, nodeName);
        }

        public void InvalidAttribute(string parentName, string nodeName, string attributeName, string validateAttributes) {
            Logger.Error("A{2} {0} contains an invalid {1}.  It may only contain: {3}.", CombineName(parentName, nodeName), attributeName, Suffix(parentName), validateAttributes);
        }

        private static string CombineName(string parent, string child) {
            return string.IsNullOrEmpty(parent) || parent == child ? child : parent + " " + (child == "add" ? "item" : child);
        }

        public void InvalidElement(string parentName, string nodeName, string subNodeName) {
            if (string.IsNullOrEmpty(parentName)) {
                Logger.Error("A{2} {0} has an invalid {1}. If you need a{2} {1}, decorate it with the [Cfg].", nodeName, subNodeName, Suffix(nodeName));
            } else {
                Logger.Error("A{2} {0} has an invalid {1}.", CombineName(parentName, nodeName), subNodeName, Suffix(parentName));
            }
        }

        public void MissingAttribute(string parentName, string nodeName, string attributeName) {
            var combinedName = CombineName(parentName, nodeName);
            Logger.Error("A{2} {0} is missing {1}.", combinedName, attributeName, Suffix(combinedName));
        }

        public void MissingRequiredAdd(string parent, string key) {
            Logger.Error($"{key} must be populated{(string.IsNullOrEmpty(parent) ? "." : " in " + parent + ".")}");
        }

        public void SettingValue(string propertyName, object value, string parentName, string nodeName, string message) {
            Logger.Error("Could not set {0} to {1} inside {2}. {3}", propertyName, value, CombineName(parentName, nodeName), message);
        }

        public void UnexpectedElement(string elementName, string subNodeName) {
            Logger.Error("There is an invalid {0} in {1}.", subNodeName, elementName);
        }

        public void ValueNotInDomain(string name, object value, string validValues) {
            Logger.Error("An invalid value of {0} is in {1}.  The valid domain is: {2}.", value, name, validValues);
        }

        public void ParseException(string message) {
            Logger.Error("Could not parse the configuration. {0}", message);
        }

        private static string Suffix(string thing) {
            return string.IsNullOrEmpty(thing) || IsVowel(thing[0]) ? "n" : string.Empty;
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

        public void ConstructorNotFound(string parentName, string name) {
            Logger.Error("{0} implementing IProperties has an incompatible constructor.  Cfg-Net needs a constructorless or single parameter constructor.  The single parameter may be a string[] of names, or an integer representing capacity.", CombineName(parentName, name));
        }

        public static string TypeMismatch(string key, object value, Type propertyType) {
            return $"The {key} default of {value} is not a {propertyType}.  Please cast it as such or change your type.";
        }

        public void ValueTooShort(string name, string value, int minLength) {
            Logger.Error("The {0} {1} is too short. It is {3} characters. It must be at least {2} characters.", name, value, minLength, value.Length);
        }

        public void ValueTooLong(string name, string value, int maxLength) {
            var val = string.IsNullOrEmpty(value) ? "(blank)" : value;
            Logger.Error("The {0} {1} is too long. It is {3} characters. It must not exceed {2} characters.", name, val, maxLength, value?.Length ?? 0);
        }

        public void ValueIsNotComparable(string name, object value) {
            var val = value == null || value.ToString() == string.Empty ? "(blank)" : value;
            Logger.Error("The {0} value {1} is not comparable.  Having a minValue or maxValue set on an incomparable property type is invalid.", name, val);
        }

        public void ValueTooSmall(string name, object value, object minValue) {
            var val = value == null || value.ToString() == string.Empty ? "(blank)" : value;
            Logger.Error("The {0} value {1} is too small. The minimum value allowed is {2}.", name, val, minValue);
        }

        public void ValueTooBig(string name, object value, object maxValue) {
            var val = value == null || value.ToString() == string.Empty ? "(blank)" : value;
            Logger.Error("The {0} value {1} is too big. The maximum value allowed is {2}.", name, val, maxValue);
        }

        public void NullOrEmptyConfiguration() {
            Error("The configuration passed in is null or empty.");
        }

        public void InvalidConfiguration(char character) {
            Error($"Without a custom parser, the configuration should be XML or JSON. Your configuration starts with {character}.");
        }

        public void Clear(IEnumerable<string> modelErrors) {
            Logger.Clear();
            foreach (var error in modelErrors) {
                Logger.Error(error);
            }
        }

        public static string InvalidRegex(string key, string regex, ArgumentException ex) {
            return $"{key} has an invalid regex of {regex} {ex.Message}";
        }

        public void ValueDoesNotMatchRegex(string name, string value, string regex) {
            Error($"{value} does not match regex {regex} in {name}");
        }
    }
}