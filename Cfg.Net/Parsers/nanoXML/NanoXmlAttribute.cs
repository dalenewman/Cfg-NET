namespace Cfg.Net.Parsers.nanoXML {
    /// <summary>
    ///     XML element attribute
    /// </summary>
    public class NanoXmlAttribute {
        internal NanoXmlAttribute(string name, string value) {
            Name = name;
            Value = value;
        }

        /// <summary>
        ///     Attribute name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Attribtue value
        /// </summary>
        public string Value { get; set; }
    }
}