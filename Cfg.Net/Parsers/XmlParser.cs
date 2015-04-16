using Transformalize.Libs.Cfg.Net.Parsers.nanoXML;

namespace Transformalize.Libs.Cfg.Net.Parsers
{
    public class NanoXmlParser : IParser
    {
        public INode Parse(string cfg)
        {
            return new XmlNode(new NanoXmlDocument(cfg).RootNode);
        }
    }
}