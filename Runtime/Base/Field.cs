using System.Reflection;

namespace UV.EzyReflection
{
    /// <summary>
    /// Defines a field 
    /// </summary>
    public class Field : Member
    {
        /// <summary>
        /// Initializes a Field with the specified field information, instance, and parent object
        /// </summary>
        /// <param name="memberInfo">The FieldInfo that provides information about this field</param>
        /// <param name="instance">The instance of the object that this field represents</param>
        /// <param name="parentObject">The parent object of this field</param>
        public Field(FieldInfo memberInfo, object instance, object parentObject)
            : base(memberInfo, instance, parentObject)
        {
            FieldInfo = memberInfo;
            MemberType = memberInfo.FieldType;
        }

        /// <summary>
        /// The reference to the field info of the field
        /// </summary>
        public FieldInfo FieldInfo { get; private set; }

        /// <summary>
        /// The current value of the field 
        /// </summary>
        /// <returns>Returns the current value of the field</returns>
        public object GetValue()
        {
            return FieldInfo.GetValue(ParentObject);
        }

        /// <summary>
        /// Sets the value of the field 
        /// </summary>
        /// <param name="value">The new value of the field</param>
        public void SetValue(object value)
        {
            FieldInfo.SetValue(ParentObject, value);
        }
    }
}
