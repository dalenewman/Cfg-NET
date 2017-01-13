using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Environment
{
    public interface IPlaceHolderReplacer {
        string Replace(string value, IDictionary<string, string> parameters, ILogger logger);
    }
}