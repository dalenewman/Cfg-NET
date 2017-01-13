using System;
using System.Collections.Generic;
using Cfg.Net.Contracts;

namespace Cfg.Net.Shared {
    public static class CfgNodeTraverser {
        public static void Traverse(IEnumerable<INode> nodes, Action<INode, IDictionary<string, string>, ILogger> action, IDictionary<string, string> parameters, ILogger logger) {
            foreach (var node in nodes) {
                action(node, parameters, logger);
                Traverse(node.SubNodes, action, parameters, logger);
            }
        }
    }
}
