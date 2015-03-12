using System;
using System.Collections.Generic;
using System.Reflection;

namespace Transformalize.Libs.Cfg.Net {

    public class CfgMetadata {

        private readonly HashSet<string> _domainSet;

        public PropertyInfo PropertyInfo { get; set; }
        public CfgAttribute Attribute { get; set; }
        public Type ListType { get; set; }
        public Func<CfgNode> Loader { get; set; }
        public string[] UniquePropertiesInList { get; set; }
        public string SharedProperty { get; set; }
        public object SharedValue { get; set; }
        public Action<object, object> Setter { get; set; }
        public Func<object, object> Getter { get; set; }
        public bool TypeMismatch { get; set; }

        public CfgMetadata(PropertyInfo propertyInfo, CfgAttribute attribute) {
            PropertyInfo = propertyInfo;
            Attribute = attribute;

            if (string.IsNullOrEmpty(attribute.domain))
                return;

            if (attribute.domainDelimiter == default(char)) {
                attribute.domainDelimiter = ',';
            }

            _domainSet = new HashSet<string>(attribute.domain.Split(new[] { attribute.domainDelimiter }, StringSplitOptions.None), attribute.ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        }

        public bool IsInDomain(string value) {
            return _domainSet == null || (value != null && _domainSet.Contains(value));
        }
    }
}