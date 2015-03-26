using System;
using System.Reflection;

namespace Transformalize.Libs.Cfg.Net {

    // Credit to http://stackoverflow.com/users/478478/alex-hope-oconnor
    public static class ReflectionHelper {

        public static Func<object, object> CreateGetter(PropertyInfo property) {
            var getter = property.GetGetMethod();
            var genericMethod = typeof(ReflectionHelper).GetMethod("CreateGetterGeneric");
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)genericHelper.Invoke(null, new object[] { getter });
        }

        public static Func<object, object> CreateGetterGeneric<T, R>(MethodInfo getter) where T : class {
            var getterTypedDelegate = (Func<T, R>)Delegate.CreateDelegate(typeof(Func<T, R>), getter);
            var getterDelegate = (Func<object, object>)((object instance) => getterTypedDelegate((T)instance));
            return getterDelegate;
        }

        public static Action<object, object> CreateSetter(PropertyInfo property) {
            var setter = property.GetSetMethod();
            var genericMethod = typeof(ReflectionHelper).GetMethod("CreateSetterGeneric");
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)genericHelper.Invoke(null, new object[] { setter });
        }

        public static Action<object, object> CreateSetterGeneric<T, V>(MethodInfo setter) where T : class {
            var setterTypedDelegate = (Action<T, V>)Delegate.CreateDelegate(typeof(Action<T, V>), setter);
            var setterDelegate = (Action<object, object>)((instance, value) => setterTypedDelegate((T)instance, value == null ? default(V) : (V)value));
            return setterDelegate;
        }

    }
}