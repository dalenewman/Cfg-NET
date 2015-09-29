using System.Xml.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.Parsers.Xml.Linq {
    public class XDocumentParser : IParser {
        public INode Parse(string cfg) {
            return new XDocumentNode(XDocument.Parse(cfg).Root);
        }

        public string Decode(string value) {
            return value;
        }
    }
}