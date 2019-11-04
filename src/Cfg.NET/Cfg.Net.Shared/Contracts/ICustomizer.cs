// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2018 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    /// <summary>
    /// One way to inject a modifier or validator (aka customize your configuration)
    /// </summary>
    public interface ICustomizer : IDependency {

        /// <summary>
        /// Customize every node (aka item, or object) using collection (aka container, or parent) as a filter (or not).
        /// </summary>
        /// <param name="collection">The collection (aka container, or parent) name passed in by Cfg-NET.</param>
        /// <param name="node">The node (aka item, or object) passed in by Cfg-NET.</param>
        /// <param name="parameters">The parameters passed in from the outside world is passed in by Cfg-NET.</param>
        /// <param name="logger">The logger used to record warnings and errors is passed in by Cfg-NET.</param>
        void Customize(string collection, INode node, IDictionary<string, string> parameters, ILogger logger);

        /// <summary>
        /// Customize the root (top-level) node just after the configuration is parsed.
        /// </summary>
        /// <param name="root">The root (top-level) node is passed in by Cfg-NET.</param>
        /// <param name="parameters">The parameters passed in from the outside world is passed in by Cfg-NET.</param>
        /// <param name="logger">The logger used to record warnings and errors is passed in by Cfg-NET.</param>
        void Customize(INode root, IDictionary<string, string> parameters, ILogger logger);
    }
}