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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cfg.Net.Shorthand {
    public class Expressions : List<Expression> {

        public Expressions(string value) {
            AddRange(Split(value).Select(e => new Expression(e)));
        }

        private static IEnumerable<string> Split(string arg, int skip = 0) {

            if (string.IsNullOrEmpty(arg))
                yield break;

            var split = arg.Replace(Utility.Escape + Utility.Close, Utility.ControlString).Split(Utility.ExpressionSplitter, StringSplitOptions.None);

            if (split.Length == 1) { // there was no split
                yield return split[0].Replace(Utility.ControlString, Utility.Close);
            } else {  // there was a split and we have to add the closer back on to where we split it
                var last = split.Length - 1;
                for (var i = 0; i < last; i++) {
                    yield return split[i].Replace(Utility.ControlString, Utility.Close) + Utility.Close;
                }
                yield return split[last].Replace(Utility.ControlString, Utility.Close);

            }
        }

    }
}