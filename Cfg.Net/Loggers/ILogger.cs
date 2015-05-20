namespace Transformalize.Libs.Cfg.Net.Loggers {
    public interface ILogger {
        void Warn(string message, params object[] args);
        void Error(string message, params object[] args);
    }

    internal class NullLogger : ILogger {
        public void Warn(string message, params object[] args) {
        }

        public void Error(string message, params object[] args) {
        }
    }
}
