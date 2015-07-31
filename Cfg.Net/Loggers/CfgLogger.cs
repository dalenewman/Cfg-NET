using System.Collections.Generic;

namespace Transformalize.Libs.Cfg.Net.Loggers {

   sealed class CfgLogger : ILogger {
      readonly MemoryLogger _memorylogger;
      readonly List<ILogger> _loggers = new List<ILogger>();

      public CfgLogger(MemoryLogger memorylogger, ILogger logger) {
         _memorylogger = memorylogger;

         _loggers.Add(memorylogger);
         if (logger != null) {
            _loggers.Add(logger);
         }

      }

      public void Warn(string message, params object[] args) {
         for (var i = 0; i < _loggers.Count; i++) {
            _loggers[i].Warn(message, args);
         }
      }

      public void Error(string message, params object[] args) {
         for (var i = 0; i < _loggers.Count; i++) {
            _loggers[i].Error(message, args);
         }
      }

      public string[] Errors() {
         return _memorylogger.Errors();
      }

      public string[] Warnings() {
         return _memorylogger.Warnings();
      }

   }
}