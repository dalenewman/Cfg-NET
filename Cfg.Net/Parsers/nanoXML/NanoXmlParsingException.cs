using System;

namespace Cfg.Net.Parsers.nanoXML
{
    internal class NanoXmlParsingException : Exception
    {
        public NanoXmlParsingException(string message) : base(message)
        {
        }
    }
}