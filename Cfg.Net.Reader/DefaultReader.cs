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
using Cfg.Net.Contracts;
using Cfg.Net.Ext;

namespace Cfg.Net.Reader {
    public class DefaultReader : IReader {
        private readonly ISourceDetector _sourceDetector;
        private readonly IReader _fileReader;
        private readonly IReader _webReader;

        public DefaultReader(
            ISourceDetector sourceDetector,
            IReader fileReader,
            IReader webReader
            ) {
            _sourceDetector = sourceDetector;
            _fileReader = fileReader;
            _webReader = webReader;
        }

        public ReaderResult Read(string resource, ILogger logger) {
            var result = new ReaderResult() { Source = _sourceDetector.Detect(resource, logger) };
            switch (result.Source) {
                case Source.File:
                    return _fileReader.Read(resource, logger);
                case Source.Url:
                    return _webReader.Read(resource, logger);
                default:
                    result.Content = resource;
                    break;
            }
            return result;
        }
    }
}