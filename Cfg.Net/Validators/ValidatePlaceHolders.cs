using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Validators {
    public class ValidatePlaceHolders : IGlobalValidator {

        private readonly char _placeHolderMarker;
        private readonly char _placeHolderOpen;
        private readonly char _placeHolderClose;

        public ValidatePlaceHolders() : this('@', '(', ')'){}

        public ValidatePlaceHolders(char placeHolderMarker, char placeHolderOpen, char placeHolderClose) {
            _placeHolderMarker = placeHolderMarker;
            _placeHolderOpen = placeHolderOpen;
            _placeHolderClose = placeHolderClose;
        }
        public ValidatorResult Validate(string name, string value, IDictionary<string, string> parameters) {
            if (value.IndexOf(_placeHolderMarker) < 0)
                return new ValidatorResult { Valid = true };

            List<string> badKeys = null;
            for (var j = 0; j < value.Length; j++) {
                if (value[j] == _placeHolderMarker && value.Length > j + 1 && value[j + 1] == _placeHolderOpen) {
                    var length = 2;
                    while (value.Length > j + length && value[j + length] != _placeHolderClose) {
                        length++;
                    }
                    if (length > 2) {
                        var key = value.Substring(j + 2, length - 2);
                        if (!parameters.ContainsKey(key)) {
                            if (badKeys == null) {
                                badKeys = new List<string> { key };
                            } else {
                                badKeys.Add(key);
                            }
                        }
                    }
                    j = j + length;
                }
            }

            if (badKeys == null)
                return new ValidatorResult { Valid = true };

            var result = new ValidatorResult { Valid = false };
            var formatted = $"{_placeHolderMarker}{_placeHolderOpen}{string.Join($"{_placeHolderClose}, {_placeHolderMarker}{_placeHolderOpen}", badKeys)}{_placeHolderClose}";
            result.Error("Missing {0} for {1}.", badKeys.Count == 1 ? "a parameter value" : "parameter values", formatted);
            return result;
        }
    }
}