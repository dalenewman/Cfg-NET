namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Target : CfgNode {
        [Cfg(required = true, unique = true, toLower = true)]
        public string Name { get; set; }

        [Cfg(required = true)]
        public string Collection { get; set; }

        [Cfg(required = true)]
        public string Property { get; set; }
    }
}