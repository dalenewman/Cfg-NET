using System.Collections.Generic;
using System.Text;
using Cfg.Net.Contracts;

namespace Cfg.Net.Environment {
    public class PlaceHolderReplacer : IPlaceHolderReplacer {

        private readonly char _placeHolderMarker;
        private readonly char _placeHolderOpen;
        private readonly char _placeHolderClose;

        public PlaceHolderReplacer() : this('@', '(', ')') { }

        public PlaceHolderReplacer(char placeHolderMarker, char placeHolderOpen, char placeHolderClose) {
            _placeHolderMarker = placeHolderMarker;
            _placeHolderOpen = placeHolderOpen;
            _placeHolderClose = placeHolderClose;
        }

        public string Replace(string str, IDictionary<string, string> parameters, ILogger logger) {

            if (str.IndexOf(_placeHolderMarker) < 0)
                return str;

            var builder = new StringBuilder();
            for (var j = 0; j < str.Length; j++) {
                if (str[j] == _placeHolderMarker && str.Length > j + 1 && str[j + 1] == _placeHolderOpen) {
                    var length = 2;
                    while (str.Length > j + length && str[j + length] != _placeHolderClose) {
                        length++;
                    }
                    if (length > 2) {
                        var key = str.Substring(j + 2, length - 2);
                        if (parameters.ContainsKey(key)) {
                            builder.Append(parameters[key]);
                        } else {
                            var placeHolder = $"@({key})";
                            builder.Append(placeHolder);
                            logger.Error($"Missing parameter for place-holder {placeHolder}.");
                        }
                    }
                    j = j + length;
                } else {
                    builder.Append(str[j]);
                }
            }

            return builder.ToString();

        }
    }
}
