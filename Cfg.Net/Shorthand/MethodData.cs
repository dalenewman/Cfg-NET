namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class MethodData {
        public MethodData(Method method, Signature signature, Target target) {
            Method = method;
            Signature = signature;
            Target = target;
        }
        public Method Method { get; set; }
        public Signature Signature { get; set; }
        public Target Target { get; set; }
    }
}