namespace Transformalize.Libs.Cfg.Net.Shorthand {
    public class Parameter : CfgNode {
        [Cfg(required = true, unique = true)]
        public string Name { get; set; }
        [Cfg]
        public string Value { get; set; }
    }
}