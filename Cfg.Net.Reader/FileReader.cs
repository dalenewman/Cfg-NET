#region license
// Cfg.Net
// Copyright 2015 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
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
    public class FileReader : IReader {

        public ReaderResult Read(string resource, ILogger logger) {
            var result = new ReaderResult { Source = Source.File };

            var queryStringIndex = resource.IndexOf('?');
            if (queryStringIndex > 0) {
                result.Parameters = HttpUtility.ParseQueryString(resource.Substring(queryStringIndex+1));
                resource = resource.Substring(0, queryStringIndex);
            }

            if (Path.HasExtension(resource)) {

                if (!Path.IsPathRooted(resource)) {
                    resource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resource);
                }

                var fileInfo = new FileInfo(resource);
                try {
                    result.Content = File.ReadAllText(fileInfo.FullName);
                } catch (Exception ex) {
                    logger.Error("Can not read file. {0}", ex.Message);
                    result.Source = Source.Error;
                }
            } else {
                logger.Error("Invalid file name: {0}.  File must have an extension (e.g. xml, json, etc)", resource);
                result.Source = Source.Error;
            }
            return result;
        }
    }
}