using System;
using System.Reflection;

namespace com.finalstudio.udi
{
    public static class TypeExtensions
    {
        public static Type GetUnderlyingType(this MemberInfo info)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) info).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) info).PropertyType;
                default:
                    throw new ArgumentOutOfRangeException($"Type {info.MemberType} not yet supported!");
            }
        }
        
        public static void SetUnderlyingType(this MemberInfo info, object target, object val)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo) info).SetValue(target, val);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo) info).SetValue(target, val);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Type {info.MemberType} not yet supported!");
            }
        }
    }
}