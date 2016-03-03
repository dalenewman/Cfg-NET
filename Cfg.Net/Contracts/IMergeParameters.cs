using System.Collections.Generic;

namespace Cfg.Net.Contracts {

    public interface IMergeParameters : IDependency {
        /// <summary>
        /// Find new parameters in `root` and merge them with `parameters`
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDictionary<string, string> Merge(INode root, IDictionary<string, string> parameters);
    }
}