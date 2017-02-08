#region license
// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2017 Dale Newman
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
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.Shorthand {
    public class ShorthandRoot : CfgNode {

        public ShorthandRoot(string cfg, params IDependency[] dependencies)
            : base(dependencies) {
            Load(cfg);
            if (!Errors().Any()) {
                InitializeMethodDataLookup();
            }
        }

        public Dictionary<string, MethodData> MethodDataLookup { get; set; } = new Dictionary<string, MethodData>(StringComparer.OrdinalIgnoreCase);

        [Cfg(required = true)]
        public List<Signature> Signatures { get; set; }

        [Cfg(required = true)]
        public List<Method> Methods { get; set; }

        protected override void Validate() {
            var signatures = Methods.Select(f => f.Signature).Distinct();
            foreach (var signature in signatures.Where(signature => Signatures.All(s => s.Name != signature))) {
                Error("The shorthand signature {0} is undefined.", signature);
            }
        }

        private void InitializeMethodDataLookup() {
            foreach (var method in Methods) {
                MethodDataLookup[method.Name] = new MethodData(method, Signatures.First(s => s.Name == method.Signature));
            }
        }
    }
}