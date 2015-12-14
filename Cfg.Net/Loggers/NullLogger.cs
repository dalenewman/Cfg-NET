using Cfg.Net.Contracts;

namespace Cfg.Net.Loggers {
    public class NullLogger : ILogger {
        public void Warn(string message, params object[] args) { }
        public void Error(string message, params object[] args) { }
    }
}