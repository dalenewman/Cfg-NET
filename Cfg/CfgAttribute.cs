using System;

namespace Transformalize.Libs.Cfg.Net
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CfgAttribute : Attribute {
        private int _minLength;
        private int _maxLength;
        private string _domain;

        // ReSharper disable InconsistentNaming
        public object value { get; set; }
        public bool required { get; set; }
        public bool unique { get; set; }
        public bool toUpper { get; set; }
        public bool toLower { get; set; }
        public string sharedProperty { get; set; }
        public object sharedValue { get; set; }

        public string domain {
            get { return _domain; }
            set {
                _domain = value;
                DomainSet = true;
            }
        }

        public char domainDelimiter { get; set; }
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

        public bool MaxLengthSet { get; private set; }
        public bool MinLengthSet { get; private set; }
        public bool DomainSet { get; private set; }

        // ReSharper restore InconsistentNaming
    }
}