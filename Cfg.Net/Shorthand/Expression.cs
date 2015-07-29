using System.Collections.Generic;
using System.Linq;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
   public class Expression {
      const char ParameterSplitter = ',';
      const char Open = '(';
      const string Close = ")";

      public string Method { get; private set; }
      public List<string> Parameters { get; private set; }
      public string OriginalExpression { get; private set; }
      public string SingleParameter { get; set; }

      public Expression(string expression) {
         OriginalExpression = expression;
         var openIndex = expression.IndexOf(Open);

         if (openIndex > 0) {
            Method = expression.Substring(0, openIndex).ToLower();
            var parameters = expression.Remove(0, openIndex + 1);
            if (parameters.EndsWith(Close, System.StringComparison.Ordinal)) {
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