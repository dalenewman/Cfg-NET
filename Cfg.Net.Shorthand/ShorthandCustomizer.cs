﻿// Cfg.Net
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

namespace Cfg.Net.Shorthand {

    public class ShorthandCustomizer : ICustomizer {

        private readonly ShorthandRoot _root;
        private readonly HashSet<string> _shortHandCollections;
        private readonly string _shortHandProperty;
        private readonly string _longHandCollection;
        private readonly string _longHandProperty;
        private Dictionary<string, MethodData> MethodDataLookup { get; set; } = new Dictionary<string, MethodData>(StringComparer.OrdinalIgnoreCase);

        public ShorthandCustomizer(
            ShorthandRoot root,
            IEnumerable<string> shortHandCollections,
            string shortHandProperty,
            string longHandCollection,
            string longHandProperty
        ) {
            _root = root;
            _shortHandCollections = new HashSet<string>(shortHandCollections, StringComparer.OrdinalIgnoreCase);
            _shortHandProperty = shortHandProperty;
            _longHandCollection = longHandCollection;
            _longHandProperty = longHandProperty;
            InitializeMethodDataLookup();
        }

        public void Customize(string collection, INode node, IDictionary<string, string> parameters, ILogger logger) {
            if (!_shortHandCollections.Contains(collection))
                return;

            var str = string.Empty;

            if (node.TryAttribute(_shortHandProperty, out var attr) && attr.Value != null) {
                str = attr.Value.ToString();
            }

            if (str == string.Empty)
                return;

            var expressions = new Expressions(str);
            var shorthandNodes = new Dictionary<string, List<INode>>();

            foreach (var expression in expressions) {

                if (!MethodDataLookup.TryGetValue(expression.Method, out var methodData)) {
                    logger.Warn($"The short-hand expression method {expression.Method} is undefined.");
                    continue;
                }

                if (methodData.Method.Ignore) {
                    continue;
                }

                var shorthandNode = new Node("add");
                shorthandNode.Attributes.Add(new ShorthandAttribute(_longHandProperty, expression.Method));

                var signatureParameters = methodData.Signature.Parameters.Select(p => new Parameter { Name = p.Name, Value = p.Value }).ToList();
                var passedParameters = expression.Parameters.Select(p => new string(p.ToCharArray())).ToArray();

                // single parameters
                if (methodData.Signature.Parameters.Count == 1 && expression.SingleParameter != string.Empty) {
                    var name = methodData.Signature.Parameters[0].Name;
                    var val = expression.SingleParameter.StartsWith(name + ":",
                        StringComparison.OrdinalIgnoreCase)
                        ? expression.SingleParameter.Remove(0, name.Length + 1)
                        : expression.SingleParameter;
                    shorthandNode.Attributes.Add(new ShorthandAttribute(name, val));
                } else {
                    // named parameters
                    if(methodData.Signature.NamedParameterIndicator != string.Empty) {
                        foreach (var parameter in passedParameters) {
                            var split = Utility.Split(parameter, methodData.Signature.NamedParameterIndicator[0]);
                            if (split.Length != 2)
                                continue;

                            var name = Utility.NormalizeName(split[0]);
                            shorthandNode.Attributes.Add(new ShorthandAttribute(name, split[1]));
                            signatureParameters.RemoveAll(p => Utility.NormalizeName(p.Name) == name);
                            var parameter1 = parameter;
                            expression.Parameters.RemoveAll(p => p == parameter1);
                        }
                    }


                    // ordered nameless parameters
                    for (var m = 0; m < signatureParameters.Count; m++) {
                        var signatureParameter = signatureParameters[m];
                        var parameterValue = m < expression.Parameters.Count ? expression.Parameters[m] : (signatureParameter.Value ?? string.Empty);


                        if (methodData.Signature.NamedParameterIndicator != string.Empty && parameterValue.Contains("\\" + methodData.Signature.NamedParameterIndicator[0])) {
                            parameterValue = parameterValue.Replace("\\" + methodData.Signature.NamedParameterIndicator[0], methodData.Signature.NamedParameterIndicator);
                        }

                        var attribute = new ShorthandAttribute(signatureParameter.Name, parameterValue);
                        shorthandNode.Attributes.Add(attribute);
                    }
                }

                if (shorthandNodes.ContainsKey(_longHandCollection)) {
                    shorthandNodes[_longHandCollection].Add(shorthandNode);
                } else {
                    shorthandNodes[_longHandCollection] = new List<INode> { shorthandNode };
                }
            }

            foreach (var pair in shorthandNodes) {
                var shorthandCollection = node.SubNodes.FirstOrDefault(sn => sn.Name == pair.Key);
                if (shorthandCollection == null) {
                    shorthandCollection = new Node(pair.Key);
                    shorthandCollection.SubNodes.AddRange(pair.Value);
                    node.SubNodes.Add(shorthandCollection);
                } else {
                    shorthandCollection.SubNodes.InsertRange(0, pair.Value);
                }
            }
        }

        public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger) { }

        private void InitializeMethodDataLookup() {
            foreach (var method in _root.Methods) {
                MethodDataLookup[method.Name] = new MethodData(method, _root.Signatures.First(s => s.Name == method.Signature));
            }
        }

    }
}