// Cfg.Net
// An Alternative .NET Configuration Handler
// Copyright 2015-2018 Dale Newman
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cfg.Net.Contracts;

namespace Cfg.Net.Reader {
   public class FileReader : IReader {

      public string Read(string fileName, IDictionary<string, string> parameters, ILogger logger) {

         if (string.IsNullOrEmpty(fileName)) {
            logger.Error("Your configuration file name is null or empty.");
            return null;
         }

         // This allows for consumer to pass in URL query parameters with file name
         var queryStringIndex = fileName.IndexOf('?');
         if (queryStringIndex > 0) {
            if(parameters == null) {
               parameters = new Dictionary<string, string>();
            }
            var urlParameters = HttpUtility.ParseQueryString(fileName.Substring(queryStringIndex + 1));
            foreach (var pair in urlParameters) {
               parameters[pair.Key] = pair.Value;
            }
            fileName = fileName.Substring(0, queryStringIndex);
         }

         if (Path.HasExtension(fileName)) {
            FileInfo fileInfo;
            if (!Path.IsPathRooted(fileName)) {
               fileName = new FileFinder().Find(fileName, logger);
            }

            fileInfo = new FileInfo(fileName);

            var intersection = fileInfo.Name.Intersect(Path.GetInvalidFileNameChars()).ToArray();

            if (intersection.Any()) {
               logger.Error("Your configuration file name contains invalid characters: " + string.Join(", ", intersection) + ".");
               return null;
            }


            try {
               return File.ReadAllText(fileInfo.FullName);
            } catch (Exception ex) {
               logger.Error("Can not read file. {0}", ex.Message);
               return null;
            }
         }

         logger.Error("Invalid file name: {0}.  File must have an extension (e.g. xml, json, etc)", fileName);
         return null;
      }
   }
}