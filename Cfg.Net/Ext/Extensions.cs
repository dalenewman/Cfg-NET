using System;

namespace Cfg.Net.Ext {

    public static class Extensions {

        [Obsolete("Use .WithDefaults() instead.  This method will be made internal in next version.")]
        public static void SetDefaults(this CfgNode node) {
            var metadata = CfgMetadataCache.GetMetadata(node.GetType(), node.Events);
            foreach (var pair in metadata) {
                if (pair.Value.PropertyInfo.PropertyType.IsGenericType) {
                    var value = pair.Value.Getter(node);
                    if (value == null) {
                        pair.Value.Setter(node, Activator.CreateInstance(pair.Value.PropertyInfo.PropertyType));
                    }
                } else {
                    if (pair.Value.TypeMismatch || pair.Value.Attribute.value == null)
                        continue;

                    var value = pair.Value.Getter(node);

                    if (value == null) {
                        pair.Value.Setter(node, pair.Value.Attribute.value);
                    } else if (value.Equals(pair.Value.Default) && !pair.Value.Default.Equals(pair.Value.Attribute.value)) {
                        pair.Value.Setter(node, pair.Value.Attribute.value);
                    }
                }
            }
        }

        /// <summary>
        /// When you want to clone yourself 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static T Clone<T>(this T node) {
            return CfgMetadataCache.Clone(node);
        }


        [Obsolete("Use new T() or T{} .WithDefaults() instead.  GetDefaultOf<T> will be removed in next version.")]
        public static T GetDefaultOf<T>(this CfgNode creator, Action<T> setter = null) where T : CfgNode {
            var node = Activator.CreateInstance(typeof(T)) as T;
            if (node == null)
                return null;
            node.SetDefaults();
            setter?.Invoke(node);
            return node;
        }

        public static T WithDefaults<T>(this T node) where T : CfgNode {
            node.SetDefaults();
            return node;
        }

        public static T WithValidation<T>(this T host, string parent = "") where T : CfgNode {
            host.WithDefaults();
            host.ValidateBasedOnAttributes();
            host.ValidateListsBasedOnAttributes(parent);
            return host;
        }

        [Obsolete("Use new T().WithValidation() instead.  GetValidatedOf<T> will be removed.")]
        public static T GetValidatedOf<T>(this CfgNode creator, Action<T> setter = null) where T : CfgNode {
            return GetDefaultOf(creator, setter).WithValidation();
        }

        [Obsolete("Use .WithValidation() instead.  This method only validates based on Cfg[] attributes, it does not run over-ridable methods: PreValidate, Validate, and PostValidate, because that would be a recipe for infinite loops.")]
        public static void ReValidate(this CfgNode node, string parent = "") {
            node.WithValidation();
        }

    }
}