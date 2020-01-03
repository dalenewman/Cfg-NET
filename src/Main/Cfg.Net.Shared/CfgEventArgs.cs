using System;
using System.Collections.Generic;

namespace Cfg.Net {
   public class CfgEventArgs : EventArgs {
      public string Source { get; set; }
      public IDictionary<string,string> Parameters { get; set; }
   }
}
