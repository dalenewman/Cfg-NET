namespace Transformalize.Libs.Cfg.Net.Parsers.nanoXML
{
    /// <summary>
    ///     XML element attribute
    /// </summary>
    public class NanoXmlAttribute {
        private readonly string _name;

        internal NanoXmlAttribute(string name, string value) {
            _name = name;
            Value = value;
        }

        /// <summary>
        ///     Attribute name
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        ///     Attribtue value
        /// </summary>
        public string Value { get; set; }
    }
}