using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UV.EzyReflection
{
    /// <summary>
    /// Defines the base member
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Initializes a new Member with the specified instance.
        /// </summary>
        /// <param name="instance">The instance of the object that this member represents.</param>
        /// <exception cref="ArgumentNullException">Thrown when the instance is null.</exception>
        public Member(object instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Name = Instance.ToString();
            MemberType = Instance.GetType();
        }

        /// <summary>
        /// Initializes a new Member with the specified instance and parent object.
        /// </summary>
        /// <param name="instance">The instance of the object that this member represents.</param>
        /// <param name="parentObject">The parent object of this member.</param>
        /// <exception cref="ArgumentNullException">Thrown when the instance is null.</exception>
        public Member(object instance, object parentObject) : this(instance)
        {
            ParentObject = parentObject ?? throw new ArgumentNullException(nameof(parentObject));
        }

        /// <summary>
        /// Initializes a new Member with the specified member information, instance, and parent object.
        /// </summary>
        /// <param name="memberInfo">The MemberInfo that provides information about this member.</param>
        /// <param name="instance">The instance of the object that this member represents.</param>
        /// <param name="parentObject">The parent object of this member.</param>
        /// <exception cref="ArgumentNullException">Thrown when the instance is null.</exception>
        public Member(MemberInfo memberInfo, object instance, object parentObject) : this(instance, parentObject)
        {
            MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));
            Name = memberInfo.Name;
        }

        /// <summary>
        /// The name of the member 
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The current instance of the object 
        /// </summary>
        public object Instance { get; protected set; }

        /// <summary>
        /// The parent object of the current instance
        /// </summary>
        public object ParentObject { get; protected set; }

        /// <summary>
        /// The type of member it is 
        /// </summary>
        public Type MemberType { get; protected set; }

        /// <summary>
        /// The attributes associated with the member
        /// </summary>
        public Attribute[] Attributes { get; protected set; }

        /// <summary>
        /// Whether the member has any attributes or not
        /// </summary>
        public virtual bool HasAttributes => Attributes != null && Attributes.Length > 0;

        /// <summary>
        /// The refernce to the current member info instance 
        /// </summary>
        public MemberInfo MemberInfo { get; protected set; }

        /// <summary>
        /// The child children of the current member
        /// </summary>
        public Member[] ChildMembers { get; protected set; }

        /// <summary>
        /// Finds and assigns all the attributes of the member
        /// </summary>
        public virtual void FindAttributes()
        {
            if(MemberInfo == null)
                Attributes = Instance.GetType().GetCustomAttributes(false).Cast<Attribute>().ToArray();
            else
                Attributes = MemberInfo.GetCustomAttributes(false).Cast<Attribute>().ToArray();
        }

        /// <summary>
        /// Finds the children of the member
        /// </summary>
        public virtual void FindChildren()
        {
            //Fetch all the children 
            FindAttributes();
            var allMembers = Instance.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            //Create an empty list to contain all children 
            var children = new List<Member>();
            for (int i = 0; i < allMembers.Length; i++)
            {
                var memberInfo = allMembers[i];

                //If it is a backingfield go to the next one
                if (memberInfo.Name.Contains("k__BackingField")) continue;

                //If it is a field 
                if (memberInfo is FieldInfo field)
                {
                    var fieldValue = field.GetValue(Instance);
                    if (fieldValue == null) continue;
                    var fieldMember = new Field(field, fieldValue, Instance);
                    fieldMember.FindAttributes();
                    children.Add(fieldMember);
                }

                //If it is a property 
                if (memberInfo is PropertyInfo property)
                {
                    var propertyValue = property.GetValue(Instance);
                    if (propertyValue == null) continue;
                    var propertyMember = new Property(property, propertyValue, Instance);
                    propertyMember.FindAttributes();
                    children.Add(propertyMember);
                }

                //If the member if a method 
                if (memberInfo is MethodInfo method)
                {
                    var methodMember = new Method(method, method, Instance);
                    methodMember.FindAttributes();
                    children.Add(methodMember);
                }
            }

            //Assign the found children to the ChildMembers 
            ChildMembers = children.ToArray();
        }

        /// <summary>
        /// Finds all the children of the member
        /// </summary>
        public virtual void FindAllChildren(int maxDepth = 10, int currentDepth = 0)
        {
            //Check if the depth has gone past the maxDepth
            if (currentDepth > maxDepth) return;

            //Find all children under this member 
            FindChildren();
            if (ChildMembers == null) return;

            //Loop through all the children
            for (int i = 0; i < ChildMembers.Length; i++)
            {
                var child = ChildMembers[i];

                //If the member is a method, a primitive type or an array; continue to the next one
                if (child is Method) continue;
                if (child.MemberType.IsPrimitive || child.MemberType.IsArray) continue;

                //If it is a Unity Component
                if (child.MemberType.IsSubclassOf(typeof(Object))) continue;

                //Find all children under the current child
                child.FindAllChildren(maxDepth, currentDepth + 1);
            }
        }

        /// <summary>
        /// The member title to be used for the string representation of the method
        /// </summary>
        /// <returns>Returns the formatted member title</returns>
        public virtual string GetMemberTitle()
        {
            return $"{Name} [{MemberType}] ({(HasAttributes ? string.Join(',', Attributes.ToList()) : "No attributes")})";
        }

        /// <summary>
        /// The string reprsentation of the Member
        /// </summary>
        /// <param name="indent">The indent to be appied for the member</param>
        /// <returns>Returns the string representation of the member with the given indent</returns>
        public virtual string GetString(int indent = 0)
        {
            var output = GetMemberTitle();
            if (ChildMembers == null || ChildMembers.Length == 0) return output;

            output += " : ";
            for (int i = 0; i < ChildMembers.Length; i++)
                output += $"\n{new string(' ', indent * 2)} - {ChildMembers[i].GetString(indent + 1)}";

            return output;
        }

        public override string ToString()
        {
            return GetString();
        }
    }
}
