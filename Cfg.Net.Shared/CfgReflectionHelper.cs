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
using System.Reflection;

namespace Cfg.Net {

    // Adapted from example by Alex Hope OConner http://stackoverflow.com/users/478478/alex-hope-oconnor
    internal static class CfgReflectionHelper {

        public static Func<object, object> CreateGetter(PropertyInfo property) {
            var getter = property.GetMethod;
            var genericMethod = typeof(CfgReflectionHelper).GetRuntimeMethod("CreateGetterGeneric", new[] { typeof(MethodInfo)});
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)genericHelper.Invoke(null, new object[] { getter });
        }

        public static Func<object, object> CreateGetterGeneric<T, R>(MethodInfo getter) where T : class {
            var getterTypedDelegate = (Func<T, R>)getter.CreateDelegate(typeof(Func<T, R>));
            var getterDelegate = (Func<object, object>)((object instance) => getterTypedDelegate((T)instance));
            return getterDelegate;
        }

        public static Action<object, object> CreateSetter(PropertyInfo property) {
            var setter = property.SetMethod;
            var method = typeof(CfgReflectionHelper).GetRuntimeMethod("CreateSetterGeneric", new[] { typeof(MethodInfo) });
            var genericHelper = method.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)genericHelper.Invoke(null, new object[] { setter });
        }

        public static Action<object, object> CreateSetterGeneric<T, V>(MethodInfo setter) where T : class {
            var setterTypedDelegate = (Action<T, V>)setter.CreateDelegate(typeof(Action<T, V>));
            var setterDelegate = (Action<object, object>)((instance, value) => setterTypedDelegate((T)instance, value == null ? default(V) : (V)value));
            return setterDelegate;
        }
    }
}