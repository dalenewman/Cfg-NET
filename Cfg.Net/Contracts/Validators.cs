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
using System.Collections;
using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public class Validators : IValidators {

        private readonly Dictionary<string, IValidator> _validators = new Dictionary<string, IValidator>();

        public Validators() { }

        public Validators(KeyValuePair<string, IValidator> validator) {
            Add(validator.Key, validator.Value);
        }

        public Validators(string name, IValidator validator) {
            Add(name, validator);
        }

        public Validators(IEnumerable<KeyValuePair<string, IValidator>> validators) {
            AddRange(validators);
        }

        public IEnumerator<KeyValuePair<string, IValidator>> GetEnumerator() {
            return _validators.GetEnumerator();
        }

        public void Add(string name, IValidator validator) {
            if (name == null || validator == null)
                return;
            _validators[name] = validator;
        }

        public void AddRange(IEnumerable<KeyValuePair<string, IValidator>> validators) {
            if (validators == null)
                return;
            foreach (var validator in validators) {
                Add(validator.Key, validator.Value);
            }
        }

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