using System.Collections.Generic;

namespace Transformalize.Libs.Cfg.Net.Parsers {

    public interface INode {
        string Name { get; }
        List<IAttribute> Attributes { get; }
        List<INode> SubNodes { get; }
        bool TryAttribute(string name, out IAttribute attr);
    }
}