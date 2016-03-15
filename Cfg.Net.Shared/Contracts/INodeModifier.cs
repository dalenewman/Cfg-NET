using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public interface INodeModifier : INamedDependency {
        void Modify(INode node, string value, IDictionary<string, string> parameters);
    }
}