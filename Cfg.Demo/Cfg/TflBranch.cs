using Transformalize.Libs.Cfg.Net;

namespace Cfg.Demo.Cfg {

    public class TflBranch : CfgNode {

        public TflBranch() {

            Property(name: "name", value: string.Empty, required: true, unique: true);
            Property(name: "run-field", value: "[default]");
            Property(name: "run-operator", value: "Equal");
            Property(name: "run-type", value: "[default]");
            Property(name: "run-value", value: string.Empty);

            Collection<TflTransform>("transforms");
        }
    }
}