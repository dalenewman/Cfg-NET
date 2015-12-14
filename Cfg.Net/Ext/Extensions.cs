using System;

namespace Cfg.Net.Ext {

    public static class Extensions {

        internal static void SetDefaults(this CfgNode node) {

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
                    } else if (value.Equals(pair.Value.Default)) {
                        if (pair.Value.Default.Equals(pair.Value.Attribute.value)) {
                            if (!pair.Value.Attribute.ValueIsSet) {
                                pair.Value.Setter(node, pair.Value.Attribute.value);
                            }
                        } else {
                            pair.Value.Setter(node, pair.Value.Attribute.value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When you want to clone yourself 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static T Clone<T>(this T node) where T : CfgNode {
            return CfgMetadataCache.Clone(node);
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

    }
}