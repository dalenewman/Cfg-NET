using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public abstract class Result {
        public IList<string> Warnings { get; set; }
        public IList<string> Errors { get; set; }
        public void Error(string format, params object[] args) {
            Errors.Add(string.Format(format, args));
        }
        public void Warn(string format, params object[] args) {
            Warnings.Add(string.Format(format, args));
        }
    }
}