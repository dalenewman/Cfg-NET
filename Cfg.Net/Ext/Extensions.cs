using System;
using System.Linq;

namespace Cfg.Net.Ext {

    public static class Extensions {

        public static void SetDefaults(this CfgNode node) {
            var metadata = CfgMetadataCache.GetMetadata(node.GetType(), node.Events);
            foreach (var pair in metadata) {
                if (pair.Value.PropertyInfo.PropertyType.IsGenericType) {
                    pair.Value.Setter(node, Activator.CreateInstance(pair.Value.PropertyInfo.PropertyType));
                } else {
                    if (!pair.Value.TypeMismatch) {
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
        public static object Clone(this CfgNode node) {
            return CfgMetadataCache.Clone(node);
        }

        /// <summary>
        /// When you want to:
        /// * create
        /// * set defaults
        /// * and set a node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        public static T GetDefaultOf<T>(this CfgNode creator, Action<T> setter = null) where T : CfgNode {
            var node = Activator.CreateInstance(typeof(T)) as T;
            if (node == null)
                return null;
            node.SetDefaults();
            setter?.Invoke(node);
            node.PreValidate();
            return node;
        }

        /// <summary>
        /// When you want to:
        /// * create
        /// * set defaults
        /// * set
        /// * pre-validate
        /// * validate based on attributes
        /// * validate
        /// * and post-validate a node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        public static T GetValidatedOf<T>(this CfgNode creator, Action<T> setter = null) where T : CfgNode {

            var node = GetDefaultOf(creator, setter);
            if (node == null)
                return null;

            ReValidate(node);

            if (node.Errors().Any()) {
                foreach (var error in node.Errors()) {
                    creator.Error(error);
                }
            }

            if (!node.Warnings().Any())
                return node;

            foreach (var warning in node.Warnings()) {
                creator.Warn(warning);
            }

            return node;
        }

        public static void ReValidate(this CfgNode node, string parent = "") {
            node.PreValidate();
            node.ValidateBasedOnAttributes();
            node.ValidateListsBasedOnAttributes(parent);
            node.Validate();
            node.PostValidate();
        }

    }
}