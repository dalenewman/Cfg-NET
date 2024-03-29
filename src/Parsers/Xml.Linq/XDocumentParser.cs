﻿// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2022 Dale Newman
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
using System.Xml.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.Parsers.Xml.Linq {
    public class XDocumentParser : IParser {
        public INode Parse(string cfg) {
            return new XDocumentNode(XDocument.Parse(cfg).Root);
        }

        public string Decode(string value) {
            return value;
        }
    }
}