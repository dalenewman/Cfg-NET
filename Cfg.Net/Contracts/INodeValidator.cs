using System.Collections.Generic;

namespace Cfg.Net.Contracts
{
    public interface INodeValidator : INamedDependency {
        ValidatorResult Validate(INode node, string value, IDictionary<string, string> parameters);
    }
}