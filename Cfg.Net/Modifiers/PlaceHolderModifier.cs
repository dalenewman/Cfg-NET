using System.Collections.Generic;
using System.Text;
using Cfg.Net.Contracts;

namespace Cfg.Net.Modifiers {
    public class PlaceHolderModifier : IGlobalModifier {
        private readonly char _placeHolderMarker;
        private readonly char _placeHolderOpen;
        private readonly char _placeHolderClose;

        public PlaceHolderModifier() : this('@', '(', ')') { }

        public PlaceHolderModifier(char placeHolderMarker, char placeHolderOpen, char placeHolderClose) {
            _placeHolderMarker = placeHolderMarker;
            _placeHolderOpen = placeHolderOpen;
            _placeHolderClose = placeHolderClose;
        }

        public string Modify(string name, string value, IDictionary<string, string> parameters) {
            if (parameters == null || value.IndexOf(_placeHolderMarker) < 0)
                return value;

            var builder = new StringBuilder();
            for (var j = 0; j < value.Length; j++) {
                if (value[j] == _placeHolderMarker && value.Length > j + 1 && value[j + 1] == _placeHolderOpen) {
                    var length = 2;
                    while (value.Length > j + length && value[j + length] != _placeHolderClose) {
                        length++;
                    }
                    if (length > 2) {
                        var key = value.Substring(j + 2, length - 2);
                        if (parameters.ContainsKey(key)) {
                            builder.Append(parameters[key]);
                        } else {
                            builder.AppendFormat("@({0})", key);
                        }
                    }
                    j = j + length;
                } else {
                    builder.Append(value[j]);
                }
            }

            return builder.ToString();
        }

    }
}
