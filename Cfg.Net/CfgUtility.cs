using System;
using System.Linq;

namespace Cfg.Net {
    internal static class CfgUtility {
        internal static string[] Split(string arg, char splitter, int skip = 0) {
            if (arg.Equals(string.Empty))
                return new string[0];

            string[] split = arg.Replace("\\" + splitter, CfgConstants.ControlString).Split(splitter);
            return
                split.Select(s => s.Replace(CfgConstants.ControlChar, splitter))
                    .Skip(skip)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }

        internal static string[] Split(string arg, string[] splitter, int skip = 0) {
            if (arg.Equals(string.Empty))
                return new string[0];

            var split = arg.Replace("\\" + splitter[0], CfgConstants.ControlString).Split(splitter, StringSplitOptions.None);
            return
                split.Select(s => s.Replace(CfgConstants.ControlString, splitter[0]))
                    .Skip(skip)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
        }
    }
}
