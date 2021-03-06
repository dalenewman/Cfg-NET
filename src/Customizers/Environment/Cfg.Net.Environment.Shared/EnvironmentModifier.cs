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
using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.Environment {

    /// <inheritdoc />
    public class EnvironmentModifier : ICustomizer {
        private readonly IPlaceHolderReplacer _placeHolderReplacer;
        private readonly string _environmentsElementName;
        private readonly string _defaultEnvironmentAttribute;
        private readonly string _environmentNameAttribute;
        private readonly string _parametersElementName;
        private readonly string _parameterNameAttribute;
        private readonly string _parameterValueAttribute;

        /// <inheritdoc />
        public EnvironmentModifier() :
            this(
                new PlaceHolderReplacer('@', '(', ')'),
                "environments",
                "environment",
                "name",
                "parameters",
                "name",
                "value"
            ) { }

        /// <inheritdoc />
        public EnvironmentModifier(IPlaceHolderReplacer replacer) :
            this(
                replacer,
                "environments",
                "environment",
                "name",
                "parameters",
                "name",
                "value"
            ) { }

        /// <inheritdoc />
        public EnvironmentModifier(
            IPlaceHolderReplacer placeHolderReplacer,
            string environmentsElementName,
            string defaultEnvironmentAttribute,
            string environmentNameAttribute,
            string parametersElementName,
            string parameterNameAttribute,
            string parameterValueAttribute
            ) {
            _placeHolderReplacer = placeHolderReplacer;
            _environmentsElementName = environmentsElementName;
            _defaultEnvironmentAttribute = defaultEnvironmentAttribute;
            _environmentNameAttribute = environmentNameAttribute;
            _parametersElementName = parametersElementName;
            _parameterNameAttribute = parameterNameAttribute;
            _parameterValueAttribute = parameterValueAttribute;
        }

        /// <inheritdoc />
        /// <summary>
        /// This gets called for each node, after the parameters have been merged
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <param name="parameters"></param>
        /// <param name="logger"></param>
        public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger) {
            if (parameters.Count == 0)
                return;

            foreach (var attribute in node.Attributes) {
                attribute.Value = _placeHolderReplacer.Replace(attribute.Value.ToString(), parameters, logger);
            }
        }

        /// <summary>
        /// This gets called first, once, and will pick the right environment and merge the parameters
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parameters"></param>
        /// <param name="logger"></param>
        public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger) {

            var environments = root.SubNodes.FirstOrDefault(n => n.Name.Equals(_environmentsElementName, StringComparison.OrdinalIgnoreCase));
            if (environments == null) {
                var rootParameters = root.SubNodes.FirstOrDefault(n => n.Name.Equals(_parametersElementName, StringComparison.OrdinalIgnoreCase));
                if (rootParameters == null)
                    return;
                if (rootParameters.SubNodes.Count == 0)
                    return;
                MergeParameters(rootParameters.SubNodes, parameters);
                return;
            }

            if (environments.SubNodes.Count == 0)
                return;

            if (environments.SubNodes.Count > 1) {
                IAttribute defaultEnvironment;
                if (!root.TryAttribute(_defaultEnvironmentAttribute, out defaultEnvironment))
                    return;

                foreach (var node in environments.SubNodes) {
                    IAttribute environmentName;
                    if (!node.TryAttribute(_environmentNameAttribute, out environmentName))
                        continue;

                    // for when the default environment is set with a place-holder (e.g. @(environment))
                    var value = _placeHolderReplacer.Replace(defaultEnvironment.Value.ToString(), parameters, logger);

                    if (!value.Equals(environmentName.Value) || node.SubNodes.Count == 0)
                        continue;

                    if (node.SubNodes[0].Name == _parametersElementName) {
                        MergeParameters(node.SubNodes[0].SubNodes, parameters);
                    }
                }

            }

            // default to first environment
            var environment = environments.SubNodes[0];
            if (environment.SubNodes.Count == 0)
                return;

            var parametersNode = environment.SubNodes[0];

            if (parametersNode.Name != _parametersElementName || environment.SubNodes.Count == 0)
                return;

            MergeParameters(parametersNode.SubNodes, parameters);
        }

        private void MergeParameters(IEnumerable<INode> nodes, IDictionary<string, string> parameters) {
            foreach (var parameter in nodes) {
                string name = null;
                object value = null;
                foreach (var attribute in parameter.Attributes) {
                    if (attribute.Name == _parameterNameAttribute) {
                        name = attribute.Value.ToString();
                    } else if (attribute.Name == _parameterValueAttribute) {
                        value = attribute.Value;
                    }
                }
                if (name != null && value != null) {
                    if (parameters.ContainsKey(name)) {  // parameter is going to set the attribute value
                        IAttribute attr;
                        if (parameter.TryAttribute(_parameterValueAttribute, out attr)) {
                            attr.Value = parameters[name];
                        }
                    } else { // attribute value is going to set the parameter
                        parameters[name] = value.ToString();
                    }
                }
            }
        }

    }
}