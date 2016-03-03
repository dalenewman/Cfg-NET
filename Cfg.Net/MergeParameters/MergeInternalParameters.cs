using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.MergeParameters {
    public class MergeInternalParameters : IMergeParameters {
        private readonly IGlobalModifier _placeHolderReplacer;
        private readonly IMergeParameters _mergeParameters;
        private readonly string _environmentsElementName;
        private readonly string _defaultEnvironmentAttribute;
        private readonly string _environmentNameAttribute;
        private readonly string _parametersElementName;

        public MergeInternalParameters(
            IGlobalModifier placeHolderReplacer,
            IMergeParameters mergeParameters): 
            this(
                placeHolderReplacer, 
                mergeParameters, 
                "environments", 
                "environment", 
                "name", 
                "parameters"
            ) { }

        public MergeInternalParameters(
            IGlobalModifier placeHolderReplacer,
            IMergeParameters mergeParameters,
            string environmentsElementName,
            string defaultEnvironmentAttribute,
            string environmentNameAttribute,
            string parametersElementName
            ) {
            _placeHolderReplacer = placeHolderReplacer;
            _mergeParameters = mergeParameters;
            _environmentsElementName = environmentsElementName;
            _defaultEnvironmentAttribute = defaultEnvironmentAttribute;
            _environmentNameAttribute = environmentNameAttribute;
            _parametersElementName = parametersElementName;
        }

        public IDictionary<string, string> Merge(INode root, IDictionary<string, string> parameters) {
            for (var i = 0; i < root.SubNodes.Count; i++) {
                var environments = root.SubNodes.FirstOrDefault(n => n.Name.Equals(_environmentsElementName, StringComparison.OrdinalIgnoreCase));
                if (environments == null)
                    continue;

                if (environments.SubNodes.Count == 0)
                    break;

                INode environment;

                if (environments.SubNodes.Count > 1) {
                    IAttribute defaultEnvironment;
                    if (!root.TryAttribute(_defaultEnvironmentAttribute, out defaultEnvironment))
                        continue;

                    for (var j = 0; j < environments.SubNodes.Count; j++) {
                        environment = environments.SubNodes[j];

                        IAttribute environmentName;
                        if (!environment.TryAttribute(_environmentNameAttribute, out environmentName))
                            continue;

                        // for when the default environment is set with a place-holder (e.g. @(environment))
                        var value = _placeHolderReplacer.Modify(_defaultEnvironmentAttribute, defaultEnvironment.Value, parameters);

                        if (!value.Equals(environmentName.Value) || environment.SubNodes.Count == 0)
                            continue;

                        if (environment.SubNodes[0].Name == _parametersElementName) {
                            return _mergeParameters.Merge(environment.SubNodes[0], parameters);
                        }
                    }
                }

                // default to first environment
                environment = environments.SubNodes[0];
                if (environment.SubNodes.Count == 0)
                    break;

                var parametersNode = environment.SubNodes[0];

                if (parametersNode.Name != _parametersElementName || environment.SubNodes.Count == 0)
                    break;

                return _mergeParameters.Merge(parametersNode, parameters);
            }

            return parameters;

        }
    }
}