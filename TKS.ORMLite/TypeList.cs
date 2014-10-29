using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using TKS.ORMLite.Attrs;
namespace TKS.ORMLite
{
    internal static class TypeList
    {
        public static readonly Type _bool = typeof(bool);
        public static readonly Type _byte = typeof(byte);
        public static readonly Type _byteArray=typeof(byte[]);
        public static readonly Type _char = typeof(char);
        public static readonly Type _DateTime = typeof(DateTime);
        public static readonly Type _decimal = typeof(decimal);
        public static readonly Type _double = typeof(double);
        public static readonly Type _float = typeof(float);
        public static readonly Type _Guid = typeof(Guid);
        public static readonly Type _HttpContext = typeof(HttpContext);
        public static readonly Type _HttpRequest = typeof(HttpRequest);
        public static readonly Type _IEnumerable = typeof(IEnumerable);
        public static readonly Type _int = typeof(int);
        public static readonly Type _ItemFieldAttribute = typeof(ItemFieldAttribute);
        public static readonly Type _PrimaryKeyAttribute = typeof(PrimaryKeyAttribute);
        public static readonly Type _long = typeof(long);
        public static readonly Type _nullable = typeof(Nullable<>);
        public static readonly Type _object = typeof(object);
        public static readonly Type _sbyte = typeof(sbyte);
        public static readonly Type _short = typeof(short);
        public static readonly Type _string = typeof(string);
        public static readonly Type _uint = typeof(uint);
        public static readonly Type _ulong = typeof(ulong);
        public static readonly Type _ushort = typeof(ushort);
        public static readonly Type _void = typeof(void);


        private static Dictionary<Type, object> DefaultValueTypes = new Dictionary<Type, object>();
        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType) return null;

            object defaultValue;
            if (DefaultValueTypes.TryGetValue(type, out defaultValue)) return defaultValue;

            defaultValue = Activator.CreateInstance(type);

            Dictionary<Type, object> snapshot, newCache;
            do
            {
                snapshot = DefaultValueTypes;
                newCache = new Dictionary<Type, object>(DefaultValueTypes);
                newCache[type] = defaultValue;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref DefaultValueTypes, newCache, snapshot), snapshot));

            return defaultValue;
        }
    }
}
