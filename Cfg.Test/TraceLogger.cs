﻿using Cfg.Net.Contracts;

namespace Cfg.Test {
   public class TraceLogger : ILogger {

      public void Error(string message, params object[] args) {
         System.Diagnostics.Trace.WriteLine(string.Format(message, args), "error");
      }

      public void Warn(string message, params object[] args) {
         System.Diagnostics.Trace.WriteLine(string.Format(message, args), "warning");
      }
   }
}