using System;
using System.Linq;
using System.Reflection;

namespace UV.EzyReflection
{
    /// <summary>
    /// Contains helper methods for reflection
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Determines whether a type is a simple type.
        /// Simple types include primitive types, enums, strings, and some common structs.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a simple type; otherwise, false.</returns>
        public static bool IsSimpleType(this Type type)
        {
            if (type == null) return false;

            return
                type.IsPrimitive ||
                type.IsEnum ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(decimal)) ||
                type.Equals(typeof(DateTime)) ||
                type.Equals(typeof(DateTimeOffset)) ||
                type.Equals(typeof(TimeSpan)) ||
                type.Equals(typeof(Array)) ||
                type.Equals(typeof(Guid));
        }

        /// <summary>
        /// Fetches the value for the current member in the given object
        /// </summary>
        /// <typeparam name="T">The type of value to be fetched</typeparam>
        /// <param name="member">The member for which the value is to be fetched</param>
        /// <param name="obj">The object which contains the member</param>
        /// <returns>Returns the value if found else the default value of T</returns>
        public static T GetValue<T>(this MemberInfo member, object obj)
        {
            var value = GetValue(member, obj);
            return value == null ? default : (T)value;
        }

        /// <summary>
        /// Fetches the value for the current member in the given object
        /// </summary>
        /// <param name="member">The member for which the value is to be fetched</param>
        /// <param name="obj">The object which contains the member</param>
        /// <returns>Returns the value if found else null</returns>
        public static object GetValue(this MemberInfo member, object obj)
        {
            try
            {
                // If the obj is a property, return its value
                if (member is PropertyInfo property)
                {
                    var type = property.PropertyType;
                    return property.GetValue(obj);
                }

                // If the obj is a field, return its value   
                if (member is FieldInfo field)
                {
                    var type = field.FieldType;
                    return field.GetValue(obj);
                }
            }
            catch { return null; }
            return null;
        }

        /// <summary>
        /// Sets the value of the given member
        /// </summary>
        /// <param name="member">The member for which the value is to be set</param>
        /// <param name="parentObject">The parent object which contains the member</param>
        /// <param name="value">The value which is to be set</param>
        public static void SetValue(this MemberInfo member, object parentObject, object value)
        {
            // If the obj is a field, set its value
            if (member is FieldInfo field)
            {
                var fieldType = field.FieldType;
                if (fieldType.IsEnum)
                    value = Enum.ToObject(fieldType, value);
                else
                    value = Convert.ChangeType(value, fieldType);

                field.SetValue(parentObject, value);
            }

            // If the obj is a property, set its value
            if (member is PropertyInfo property)
            {
                var propertyType = property.PropertyType;
                try
                {
                    if (propertyType.IsEnum)
                        value = Enum.ToObject(propertyType, value);
                    else
                        value = Convert.ChangeType(value, propertyType);
                }
                catch { }

                UnityEngine.Debug.Log($"Setting value to : {member} => {value} [{propertyType}]");
                property.SetValue(parentObject, value, null);
            }
        }

        /// <summary>
        /// Changes the type of the object to the target type 
        /// </summary>
        /// <param name="obj">The object which is to be converted </param>
        /// <param name="targetType">The target type to which the object is to be converted to</param>
        /// <returns>Returns the object casted into the targetType</returns>
        public static object ChangeType(this object obj, Type targetType)
        {
            if (obj == null) return default;
            return Convert.ChangeType(obj, targetType);
        }


        /// <summary>
        /// Finds and returns the backing field for the given property 
        /// </summary>
        /// <param name="property">The property for which the backing field is to be fetched</param>
        /// <returns>Returns the field if found else null</returns>
        public static FieldInfo GetBackingField(this PropertyInfo property)
        {
            return property.DeclaringType.GetField($"<{property.Name}>k__BackingField",
                                                  BindingFlags.Public
                                                  | BindingFlags.NonPublic
                                                  | BindingFlags.Instance);
        }

        /// <summary>
        /// Fetches all the attributes on the given member info
        /// </summary>
        /// <param name="memberInfo">The memberinfo for which the attributes are to be fetched</param>
        /// <returns>Returs the attributes on the given memberinfo</returns>
        public static Attribute[] GetAttributes(this MemberInfo memberInfo)
        {
            if (memberInfo == null) return Array.Empty<Attribute>();
            var attributes = Attribute.GetCustomAttributes(memberInfo, true).Cast<Attribute>();
            if (memberInfo is PropertyInfo property)
            {
                ////Try finding the backing field of the property
                var field = GetBackingField(property);
                if (field != null)
                    attributes = attributes.Concat(field.GetCustomAttributes<Attribute>(true));
            }

            return attributes.ToArray();
        }

        /// <summary>
        /// Checks whether the current memberInfo has an attribute of the given type
        /// </summary>
        /// <typeparam name="T">The type of attribute which is to be searched</typeparam>
        /// <param name="memberInfo">The memberInfo on which the attribute is to be searched</param>
        /// <param name="parentObject">The parent object which contains the memberInfo</param>
        /// <returns>Returns true or false based on if the attribute is present on the member or not</returns>
        public static bool HasAttribute<T>(this MemberInfo memberInfo, object parentObject = null) where T : Attribute
        {
            var attributes = GetAttributes(memberInfo);

            for (int i = 0; i < attributes.Length; i++)
            {
                //Check if the current attribute is of type T
                var att = attributes[i];
                if (att == null) continue;
                if (att.GetType().Equals(typeof(T))) return true;
            }

            return false;
        }
    }
}
