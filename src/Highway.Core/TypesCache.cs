using System;
using System.Reflection;

namespace Highway.Core
{
    //TODO
    public class TypesCache : ITypesCache
    {
        public Type GetGeneric(Type baseType, params Type[] args)
        {
            return baseType.MakeGenericType(args);
        }

        public MethodInfo GetMethod(Type type, string name, Type[] args = null)
        {
            var method = (args is null) ? type.GetMethod(name) : type.GetMethod(name, args);
            return method;
        }

        public PropertyInfo GetProperty(Type type, string name)
        {
            var property = type.GetProperty(name);
            return property;
        }
    }
}