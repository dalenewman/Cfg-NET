using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    /// <summary>
    /// A modifier run on every attribute / property in your configuration
    /// </summary>
    public interface IGlobalModifier : IDependency {
        string Modify(string name, string value, IDictionary<string, string> parameters);
    }
}