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
                if (propertyType.IsEnum)
                    value = Enum.ToObject(propertyType, value);
                else
                    value = Convert.ChangeType(value, propertyType);

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
        /// <param name="parentObject">The object that contains the memberInfo</param>
        /// <returns>Returns the field if found else null</returns>
        public static FieldInfo GetBackingField(this PropertyInfo property, object parentObject)
        {
            return parentObject.GetType().GetField($"<{property.Name}>k__BackingField",
                                                  BindingFlags.Public
                                                  | BindingFlags.NonPublic
                                                  | BindingFlags.Instance);
        }

        /// <summary>
        /// Fetches all the attributes on the given member info
        /// </summary>
        /// <param name="memberInfo">The memberinfo for which the attributes are to be fetched</param>
        /// <param name="parentObject">The object that contains the memberInfo</param>
        /// <returns>Returs the attributes on the given memberinfo</returns>
        public static Attribute[] GetAttributes(this MemberInfo memberInfo, object parentObject = null)
        {
            if (memberInfo == null) return Array.Empty<Attribute>();
            var members = memberInfo.GetCustomAttributes(false).Cast<Attribute>();

            if (memberInfo is PropertyInfo && parentObject != null)
            {
                //Try finding the backing field of the property
                var field = parentObject.GetType().GetField($"<{memberInfo.Name}>k__BackingField",
                                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    members.Concat(field.GetAttributes());
            }

            return members.ToArray();
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
            var attributes = GetAttributes(memberInfo, parentObject);

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
