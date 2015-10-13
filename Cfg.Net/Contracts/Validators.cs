using System.Collections;
using System.Collections.Generic;

namespace Cfg.Net.Contracts {
    public class Validators : IValidators {

        private readonly Dictionary<string, IValidator> _validators = new Dictionary<string, IValidator>();

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