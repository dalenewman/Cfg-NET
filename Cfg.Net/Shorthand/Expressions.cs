using System.Collections.Generic;
using System.Linq;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Expressions : List<Expression> {
        private static readonly char[] ExpressionSplitter = { ')', '.' };

        public Expressions(string value) {
            AddRange(CfgNode.Split(value, ExpressionSplitter).Select(e => new Expression(e)));
        }
    }
}