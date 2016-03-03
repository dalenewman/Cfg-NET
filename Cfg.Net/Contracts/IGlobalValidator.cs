using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public interface IGlobalValidator : IDependency {
        ValidatorResult Validate(string name, string value, IDictionary<string, string> parameters);
    }
}