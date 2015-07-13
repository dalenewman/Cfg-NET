using Transformalize.Libs.Cfg.Net.Parsers;

namespace Transformalize.Libs.Cfg.Net.Shorthand {
    internal class ShorthandAttribute : IAttribute {
        public ShorthandAttribute(string name, string value) {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}