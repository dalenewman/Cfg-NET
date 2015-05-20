using System;
using System.Linq;
using System.Text;

namespace Transformalize.Libs.Cfg.Net.Loggers {

    sealed class MemoryLogger : ILogger {

        private readonly StringBuilder _errors;
        private readonly StringBuilder _warnings;
        private string[] _errorCache;
        private string[] _warningCache;

        public MemoryLogger() {
            _errors = new StringBuilder();
            _warnings = new StringBuilder();
        }

        public void Warn(string message, params object[] args) {
            _warnings.AppendFormat(message, args);
            _warnings.AppendLine();
            _warningCache = null;
        }

        public void Error(string message, params object[] args) {
            _errors.AppendFormat(message, args);
            _errors.AppendLine();
            _errorCache = null;
        }

        public string[] Errors() {
            return _errorCache ?? (_errorCache = _errors.ToString()
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray());
        }

        public string[] Warnings() {
            return _warningCache ?? (_warningCache = _warnings.ToString()
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray());
        }

    }
}