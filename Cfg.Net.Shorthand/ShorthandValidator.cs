using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Shorthand {
    public class ShorthandValidator : ICustomizer {
        private readonly ShorthandRoot _root;

        public ShorthandValidator(ShorthandRoot root) {
            _root = root;
        }

        public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger) {
            if (parent != "fields" && parent != "calculated-fields")
                return;

            var value = string.Empty;

            IAttribute attr;
            if (node.TryAttribute("t", out attr) && attr.Value != null) {
                value = attr.Value as string;
            }

            if (string.IsNullOrEmpty(value))
                return;
            var expressions = new Expressions(value);
            foreach (var expression in expressions) {
                MethodData methodData;
                if (!_root.MethodDataLookup.TryGetValue(expression.Method, out methodData)) {
                    logger.Warn($"The short-hand expression method {expression.Method} is undefined.");
                }
            }
        }

        public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger){}
    }
}