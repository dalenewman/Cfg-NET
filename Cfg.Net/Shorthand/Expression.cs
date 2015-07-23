using System.Collections.Generic;
using System.Linq;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Expression {
        static readonly char[] ParameterSplitter = { ',' };

        public string Method { get; private set; }
        public List<string> Parameters { get; private set; }
        public string OriginalExpression { get; private set; }
        public string SingleParameter { get; set; }

        public Expression(string expression) {
            OriginalExpression = expression;
            var openIndex = expression.IndexOf('(');

            if (openIndex > 0) {
                Method = expression.Substring(0, openIndex).ToLower();
                var parameters = expression.Remove(0, openIndex + 1);
                if (parameters.EndsWith(")", System.StringComparison.Ordinal)) {
                    parameters = parameters.Substring(0, parameters.Length - 1);
                }
                SingleParameter = parameters;
                Parameters = CfgNode.Split(parameters, ParameterSplitter).ToList();
            } else {
                Method = expression;
                SingleParameter = string.Empty;
                Parameters = new List<string>();
            }
        }
    }
}