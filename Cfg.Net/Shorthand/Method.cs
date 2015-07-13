namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Method : CfgNode {

        [Cfg(required = true, unique = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(required = true, toLower = true)]
        public string Signature { get; set; }

        [Cfg(required = true, toLower = true)]
        public string Target { get; set; }
    }
}