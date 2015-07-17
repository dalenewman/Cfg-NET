using System.Collections.Generic;
using System.Linq;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Expression {
        private static readonly char[] ParameterSplitter = { ',' };

        public string Method { get; private set; }
        public List<string> Parameters { get; private set; }
        public string OriginalExpression { get; private set; }

        public Expression(string expression) {
            OriginalExpression = expression;
            var openIndex = expression.IndexOf('(');

            if (openIndex > 0) {
                Method = expression.Substring(0, openIndex).ToLower();
                var parameters = expression.Remove(0, openIndex + 1);
                if (parameters.EndsWith(")")) {
                    parameters = parameters.Substring(0, parameters.Length - 1);
                }
                Parameters = CfgNode.Split(parameters, ParameterSplitter).ToList();
            } else {
                Method = expression;
                Parameters = new List<string>();
            }
        }
    }
}