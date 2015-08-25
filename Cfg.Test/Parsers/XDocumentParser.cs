using System.Xml.Linq;
using Cfg.Net.Contracts;
using Cfg.Net.Parsers;

namespace Cfg.Test.Parsers {
    public class XDocumentParser : IParser {
        public INode Parse(string cfg) {
            return new XDocumentNode(XDocument.Parse(cfg).Root);
        }
    }
}