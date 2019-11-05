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
   public class ParameterModifier : ICustomizer {
      private readonly IPlaceHolderReplacer _placeHolderReplacer;
      private readonly string _parametersElementName;
      private readonly string _parameterNameAttribute;
      private readonly string _parameterValueAttribute;

      /// <inheritdoc />
      public ParameterModifier() :
          this(
              new PlaceHolderReplacer('@', '(', ')'),
              "parameters",
              "name",
              "value"
          ) { }

      /// <inheritdoc />
      public ParameterModifier(IPlaceHolderReplacer replacer) :
          this(
              replacer,
              "parameters",
              "name",
              "value"
          ) { }

      /// <inheritdoc />
      public ParameterModifier(
          IPlaceHolderReplacer placeHolderReplacer,
          string parametersElementName,
          string parameterNameAttribute,
          string parameterValueAttribute
          ) {
         _placeHolderReplacer = placeHolderReplacer;
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
      /// This gets called first to merge the parameters that are passed in with the defaults
      /// </summary>
      /// <param name="root"></param>
      /// <param name="parameters"></param>
      /// <param name="logger"></param>
      public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger) {

         var rootParameters = root.SubNodes.FirstOrDefault(n => n.Name.Equals(_parametersElementName, StringComparison.OrdinalIgnoreCase));
         if (rootParameters == null)
            return;
         if (rootParameters.SubNodes.Count == 0)
            return;

         MergeParameters(rootParameters.SubNodes, parameters);
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