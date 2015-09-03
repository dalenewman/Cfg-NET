using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public interface IValidators : IDependency, IEnumerable<KeyValuePair<string, IValidator>> {
        void Add(string name, IValidator validator);
        void AddRange(IEnumerable<KeyValuePair<string, IValidator>> validators);
        void Remove(string name);
    }
}