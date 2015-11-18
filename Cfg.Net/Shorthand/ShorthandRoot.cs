#region License
// Cfg-NET An alternative .NET configuration handler.
// Copyright 2015 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
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

        public ShorthandRoot(string cfg, IReader reader = null, IParser parser = null)
            : base(reader, parser) {
            Load(cfg);
        }

        public Dictionary<string, MethodData> MethodDataLookup { get; set; }

        [Cfg(required = true)]
        public List<Signature> Signatures { get; set; }

        [Cfg(required = true)]
        public List<Target> Targets { get; set; }

        [Cfg(required = true)]
        public List<Method> Methods { get; set; }

        protected internal override void Validate() {
            IEnumerable<string> signatures = Methods.Select(f => f.Signature).Distinct();
            foreach (string signature in signatures.Where(signature => Signatures.All(s => s.Name != signature))) {
                Error("The shorthand signature {0} is undefined.", signature);
            }
            IEnumerable<string> targets = Methods.Select(f => f.Target).Distinct();
            foreach (string target in targets.Where(target => Targets.All(t => t.Name != target))) {
                Error("The shorthand target {0} is undefined.", target);
            }
        }

        public void InitializeMethodDataLookup() {
            MethodDataLookup = new Dictionary<string, MethodData>(StringComparer.OrdinalIgnoreCase);
            foreach (var method in Methods) {
                MethodDataLookup[method.Name] = new MethodData(
                    method,
                    Signatures.First(s => s.Name == method.Signature),
                    Targets.First(t => t.Name == method.Target)
                );
            }
        }
    }
}