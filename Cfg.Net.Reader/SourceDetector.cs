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
using System;
using System.IO;
using Cfg.Net.Contracts;

namespace Cfg.Net.Reader {
    public class SourceDetector : ISourceDetector {

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

            if (resource.StartsWith("<"))
                return Source.Xml;

            if (resource.StartsWith("{"))
                return Source.Json;

            try {
                return new Uri(resource).IsFile ? Source.File : Source.Url;
            } catch (Exception) {

                var queryStringIndex = resource.IndexOf('?');
                if (queryStringIndex > 0) {
                    resource = resource.Substring(0, queryStringIndex);
                }

                var fileName = Path.GetFileName(resource);
                if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                    logger.Error("Your configuration contains invalid characters");
                    return Source.Error;
                }
                if (!Path.HasExtension(resource)) {
                    logger.Error("Your file needs an extension.");
                    return Source.Error;
                }

                if (new FileInfo(resource).Exists) {
                    return Source.File;
                }

                logger.Error("This source detector can not detect your configuration source.  The configuration you passed in does not appear to be JSON, XML, a Uri, or a file name.");
                return Source.Error;
            }

        }
    }
}