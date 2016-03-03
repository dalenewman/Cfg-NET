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

namespace Cfg.Net {

    [AttributeUsage(AttributeTargets.Property)]
    public class CfgAttribute : Attribute {
        private string _domain;
        private int _maxLength;
        private object _maxValue;
        private int _minLength;
        private object _minValue;
        private string _validators;
        private string _modifiers;
        private object _value;

        // ReSharper disable InconsistentNaming
        public object value {
            get { return _value; }
            set {
                if (value == null) return;
                _value = value;
                ValueIsSet = true;
            }
        }

        public bool required { get; set; }
        public bool unique { get; set; }
        public bool toUpper { get; set; }
        public bool toLower { get; set; }
        public bool shorthand { get; set; }

        public bool serialize { get; set; } = true;

        public string domain {
            get { return _domain; }
            set {
                _domain = value;
                DomainSet = true;
            }
        }

        public string validators {
            get { return _validators; }
            set {
                if (value == null) return;
                _validators = value;
                ValidatorsSet = true;
            }
        }

        public string modifiers
        {
            get { return _modifiers; }
            set
            {
                if (value == null) return;
                _modifiers = value;
                ModifiersSet = true;
            }
        }

        public char delimiter { get; set; } = ',';

        [Obsolete("Use delimiter instead.")]
        public char domainDelimiter { get; set; } = ',';

        [Obsolete("Use delimiter instead.")]
        public char validatorDelimiter { get; set; } = ',';

        public bool ignoreCase { get; set; }

        public int minLength {
            get { return _minLength; }
            set {
                _minLength = value;
                MinLengthSet = true;
            }
        }

        public int maxLength {
            get { return _maxLength; }
            set {
                _maxLength = value;
                MaxLengthSet = true;
            }
        }

        public object minValue {
            get { return _minValue; }
            set {
                if (value == null) return;
                _minValue = value;
                MinValueSet = true;
            }
        }

        public object maxValue {
            get { return _maxValue; }
            set {
                if (value == null) return;
                _maxValue = value;
                MaxValueSet = true;
            }
        }

        public bool MaxLengthSet { get; private set; }
        public bool MinLengthSet { get; private set; }
        public bool MaxValueSet { get; private set; }
        public bool MinValueSet { get; private set; }
        public bool DomainSet { get; private set; }
        public bool ValidatorsSet { get; private set; }
        public bool ModifiersSet { get; private set; }
        public bool ValueIsSet { get; private set; }

        // ReSharper restore InconsistentNaming
    }
}