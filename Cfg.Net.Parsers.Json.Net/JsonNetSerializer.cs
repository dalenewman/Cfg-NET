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
using Cfg.Net.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cfg.Net.Parsers.Json.Net {
    public class JsonNetSerializer : ISerializer {
        private readonly JsonSerializerSettings _settings;

        public JsonNetSerializer() {
            _settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        public string Serialize(CfgNode node) {
            return JsonConvert.SerializeObject(node, _settings);
        }
    }
}