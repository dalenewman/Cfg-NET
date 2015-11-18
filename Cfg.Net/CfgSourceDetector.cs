#region License
// Cfg-NET An alternative .NET configuration handler.
// Copyright 2015 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Cfg.Net.Contracts;

namespace Cfg.Net {

    internal sealed class CfgSourceDetector : ISourceDetector {

        public Source Detect(string resource, ILogger logger) {
            if (resource == null) {
                logger.Error("The configuration passed in is null.");
                return Source.Error;
            }

            resource = resource.TrimStart();

            if (resource == string.Empty) {
                logger.Error("The configuration passed in is empty");
                return Source.Error;
            }

            if (resource.StartsWith("<", System.StringComparison.Ordinal))
                return Source.Xml;

            if (resource.StartsWith("{", System.StringComparison.Ordinal)) {
                return Source.Json;
            }

            logger.Error("Can not determine source for configuration. This source detector looks for XML and/or JSON.  The XML should start with <, and the JSON should start with {.  Your configuration starts with {0}.", resource.Substring(0, 1));
            return Source.Error;

        }
    }
}