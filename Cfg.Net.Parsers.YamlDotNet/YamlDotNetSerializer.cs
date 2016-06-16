using System.IO;
using Cfg.Net.Contracts;
using YamlDotNet.Serialization;

namespace Cfg.Net.Parsers.YamlDotNet {
    public class YamlDotNetSerializer : ISerializer {
        private readonly StringWriter _writer;
        private readonly Serializer _serializer;

        public YamlDotNetSerializer(
            SerializationOptions serializationOptions = SerializationOptions.None,
            INamingConvention namingConvention = null,
            YamlAttributeOverrides yamlAttributeOverrides = null
            ) {
            _writer = new StringWriter();
            _serializer = new Serializer(serializationOptions, namingConvention, yamlAttributeOverrides);
        }

        public string Serialize(CfgNode node) {
            _serializer.Serialize(_writer, node);
            return _writer.ToString();
        }
    }
}