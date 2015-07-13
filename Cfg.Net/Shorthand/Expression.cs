namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Expression {
        private static readonly char[] ParameterSplitter = { ',' };

        public string Method { get; private set; }
        public string[] Parameters { get; private set; }
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
                Parameters = CfgNode.Split(parameters, ParameterSplitter);
            } else {
                Method = expression;
                Parameters = new string[0];
            }
        }
    }
}