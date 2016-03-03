using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public interface IGlobalModifier : IDependency {
        string Modify(string name, string value, IDictionary<string, string> parameters);
    }
}