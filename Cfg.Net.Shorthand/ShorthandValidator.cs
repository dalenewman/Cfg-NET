using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Shorthand {
    public class ShorthandValidator : INodeValidator {
        private readonly ShorthandRoot _root;

        public ShorthandValidator(ShorthandRoot root, string name) {
            _root = root;
            Name = name;
        }

        public string Name { get; set; }
        public ValidatorResult Validate(INode node, string value, IDictionary<string, string> parameters) {
            var result = new ValidatorResult { Valid = true };
            var expressions = new Expressions(value);
            foreach (var expression in expressions) {
                MethodData methodData;
                if (!_root.MethodDataLookup.TryGetValue(expression.Method, out methodData)) {
                    result.Warn($"The short-hand expression method {expression.Method} is undefined.");
                }
            }
            return result;
        }
    }
}