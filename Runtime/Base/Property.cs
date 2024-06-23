using System;
using System.Linq;
using System.Reflection;

namespace UV.EzyReflection
{
    /// <summary>
    /// Defines a property 
    /// </summary>
    public class Property : Member
    {
        /// <summary>
        /// Initializes a Property with the specified property information, instance, and parent object
        /// </summary>
        /// <param name="memberInfo">The PropertyInfo that provides information about this property</param>
        /// <param name="instance">The instance of the object that this property represents</param>
        /// <param name="parentObject">The parent object of this property</param>
        public Property(PropertyInfo memberInfo, object instance, object parentObject)
            : base(memberInfo, instance, parentObject)
        {
            PropertyInfo = memberInfo;
            MemberType = memberInfo.PropertyType;

            //Try finding the backing field of the property
            var field = ParentObject.GetType().GetField($"<{Name}>k__BackingField",
                                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                        ?? throw new Exception("Non auto implemented properties are not supported!");
            BackingField = new Field(field, Instance, ParentObject);
        }

        /// <summary>
        /// The reference to the property info of the member
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// The backing field associated with this property
        /// </summary>  
        public Field BackingField { get; private set; }

        /// <summary>
        /// Finds the attributes on the property and also on the BackingField
        /// </summary>
        public override void FindAttributes()
        {
            base.FindAttributes();
            BackingField.FindAttributes();
            Attributes = Attributes.Concat(BackingField.Attributes).ToArray();
        }

        /// <summary>
        /// The current value of the property 
        /// </summary>
        /// <returns>Returns the current value of the property</returns>
        public object GetValue()
        {
            return PropertyInfo.GetValue(ParentObject);
        }

        /// <summary>
        /// Sets the value of the property 
        /// </summary>
        /// <param name="value">The new value of the property</param>
        public void SetValue(object value)
        {
            PropertyInfo.SetValue(ParentObject, value);
        }
    }
}
