using System;

namespace Transformalize.Libs.Cfg.Net {

    [AttributeUsage(AttributeTargets.Property)]
    public class CfgAttribute : Attribute {

        private int _minLength;
        private int _maxLength;
        private object _minValue;
        private object _maxValue;
        private string _domain;
        private object _value;
        private string _validators;

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
        public string sharedProperty { get; set; }
        public object sharedValue { get; set; }

        public string domain {
            get { return _domain; }
            set {
                _domain = value;
                DomainSet = true;
                NeedString = true;
            }
        }

        public string validators {
            get { return _validators; }
            set {
                _validators = value;
                ValidatorsSet = true;
            }
        }

        public char domainDelimiter { get; set; }
        public char validatorDelimiter { get; set; }
        public bool ignoreCase { get; set; }

        public int minLength {
            get { return _minLength; }
            set {
                _minLength = value;
                MinLengthSet = true;
                NeedString = true;
            }
        }

        public int maxLength {
            get { return _maxLength; }
            set {
                _maxLength = value;
                MaxLengthSet = true;
                NeedString = true;
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
        public bool NeedString { get; private set; }
        public bool ValueIsSet { get; private set; }

        // ReSharper restore InconsistentNaming
    }
}