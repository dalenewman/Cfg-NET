using System.Collections.Generic;

namespace Cfg.Net.Contracts {

    public interface IModifier : INamedDependency {
        string Modify(string name, string value, IDictionary<string, string> parameters);
    }
}