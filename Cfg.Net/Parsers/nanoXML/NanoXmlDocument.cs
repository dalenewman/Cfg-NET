using System.Collections.Generic;

namespace Cfg.Net.Parsers.nanoXML
{
    /// <summary>
    ///     Class representing whole DOM XML document
    /// </summary>
    public class NanoXmlDocument : NanoXmlBase
    {
        private readonly List<NanoXmlAttribute> _declarations = new List<NanoXmlAttribute>();
        private readonly NanoXmlNode _rootNode;

        /// <summary>
        ///     Public constructor. Loads xml document from raw string
        /// </summary>
        /// <param name="xmlString">String with xml</param>
        public NanoXmlDocument(string xmlString)
        {
            int i = 0;

            while (true)
            {
                SkipSpaces(xmlString, ref i);

                if (xmlString[i] != '<')
                    throw new NanoXmlParsingException("Unexpected token");

                i++; // skip <

                if (xmlString[i] == '?') // declaration
                {
                    i++; // skip ?
                    ParseAttributes(xmlString, ref i, _declarations, '?', '>');
                    i++; // skip ending ?
                    i++; // skip ending >

                    continue;
                }

                if (xmlString[i] == '!') // doctype
                {
                    while (xmlString[i] != '>') // skip doctype
                        i++;

                    i++; // skip >

                    continue;
                }

                _rootNode = new NanoXmlNode(xmlString, ref i);
                break;
            }
        }

        /// <summary>
        ///     Root document element
        /// </summary>
        public NanoXmlNode RootNode
        {
            get { return _rootNode; }
        }
    }
}