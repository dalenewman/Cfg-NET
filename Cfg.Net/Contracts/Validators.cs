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
using System.Collections;
using System.Collections.Generic;

namespace Cfg.Net.Contracts {

    /// <summary>
    /// A default IValidators implementation
    /// </summary>
    [Obsolete]
    public class Validators : IValidators {

        private readonly Dictionary<string, IValidator> _validators = new Dictionary<string, IValidator>();

        public Validators() { }


        public Validators(KeyValuePair<string, IValidator> validator) {
            Add(validator.Key, validator.Value);
        }

        /// <summary>
        /// Create a Validators with one validator in it (to start with).
        /// </summary>
        /// <param name="name">The name of the validator that matches the name in validators attribute.</param>
        /// <param name="validator">The validator implementation.</param>
        public Validators(string name, IValidator validator) {
            Add(name, validator);
        }

        /// <summary>
        /// Create a Validators with one or more validators in it (to start with).
        /// </summary>
        /// <param name="validators"></param>
        public Validators(IEnumerable<KeyValuePair<string, IValidator>> validators) {
            if (validators == null)
                return;
            foreach (var validator in validators) {
                Add(validator.Key, validator.Value);
            }
        }

        public IEnumerator<KeyValuePair<string, IValidator>> GetEnumerator() {
            return _validators.GetEnumerator();
        }

        public void Add(string name, IValidator validator) {
            if (name == null || validator == null)
                return;
            _validators[name] = validator;
        }

        [Obsolete("This goes un-used 99% of the time.")]
        public void AddRange(IEnumerable<KeyValuePair<string, IValidator>> validators) {
            if (validators == null)
                return;
            foreach (var validator in validators) {
                Add(validator.Key, validator.Value);
            }
        }

        [Obsolete("This goes un-used 99% of the time.")]
        public void Remove(string name) {
            if (name == null)
                return;

            if (_validators.ContainsKey(name)) {
                _validators.Remove(name);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}