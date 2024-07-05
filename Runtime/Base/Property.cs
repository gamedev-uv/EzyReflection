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
            var field = PropertyInfo.GetBackingField(parentObject);
            if (field != null)
                BackingField = new(field, Instance, ParentObject);
        }

        /// <summary>
        /// The reference to the property info of the member
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// The backing field associated with this property
        /// </summary>  
        public Field BackingField { get; private set; }
    }
}