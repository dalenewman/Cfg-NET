using System.Collections.Generic;

namespace Transformalize.Libs.Cfg.Net {
    public class CfgValidatorResult {

        public bool Valid { get; set; }
        public object Value { get; set; }
        public IList<string> Warnings { get; set; }
        public IList<string> Errors { get; set; }

        public CfgValidatorResult() {
            Valid = true;
            Value = string.Empty;
            Warnings = new List<string>();
            Errors = new List<string>();
        }

        public CfgValidatorResult(object value) {
            Valid = true;
            Value = value;
            Warnings = new List<string>();
            Errors = new List<string>();
        }

        public void Error(string format, params object[] args) {
            Errors.Add(string.Format(format, args));
        }

        public void Warn(string format, params object[] args) {
            Warnings.Add(string.Format(format, args));
        }

    }
}