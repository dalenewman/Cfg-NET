using System;
using System.Collections.Generic;
using System.Linq;

namespace Transformalize.Libs.Cfg.Net.Shorthand {

    public class ShorthandRoot : CfgNode {

        public Dictionary<string, MethodData> MethodDataLookup { get; set; }

        public ShorthandRoot(string cfg) {
            Load(cfg);
        }

        [Cfg(required = true)]
        public List<Signature> Signatures { get; set; }

        [Cfg(required = true)]
        public List<Target> Targets { get; set; }

        [Cfg(required = true)]
        public List<Method> Methods { get; set; }

        protected override void Validate() {
            var signatures = Methods.Select(f => f.Signature).Distinct();
            foreach (var signature in signatures.Where(signature => Signatures.All(s => s.Name != signature))) {
                Error("The shorthand signature {0} is undefined.", signature);
            }
            var targets = Methods.Select(f => f.Target).Distinct();
            foreach (var target in targets.Where(target => Targets.All(t => t.Name != target))) {
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
