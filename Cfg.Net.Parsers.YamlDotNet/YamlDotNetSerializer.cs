#region license
// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.IO;
using Cfg.Net.Contracts;
using YamlDotNet.Serialization;

namespace Cfg.Net.Parsers.YamlDotNet {
    public class YamlDotNetSerializer : ISerializer {
        private readonly StringWriter _writer;
        private readonly Serializer _serializer;

        public YamlDotNetSerializer(
            SerializationOptions serializationOptions = SerializationOptions.None,
            INamingConvention namingConvention = null) {
            _writer = new StringWriter();
            _serializer = new Serializer(serializationOptions, namingConvention);
        }

        public string Serialize(CfgNode node) {
            _serializer.Serialize(_writer, node);
            return _writer.ToString();
        }
    }
}