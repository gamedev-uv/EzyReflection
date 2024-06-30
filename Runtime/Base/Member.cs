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
        public Member() { }

        /// <summary>
        /// Initializes a new Member with the specified instance.
        /// </summary>
        /// <param name="instance">The instance of the object that this member represents.</param>
        /// <exception cref="ArgumentNullException">Thrown when the instance is null.</exception>
        public Member(object instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Name = Instance.ToString();
            Path = Name;
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
        /// The path of the member
        /// </summary>
        public string Path { get; protected set; }

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
        /// Fetches the value of the current member
        /// </summary>
        /// <typeparam name="T">The type of value which is to be fetched</typeparam>
        /// <returns>The value if fetched else null</returns>
        public T GetValue<T>()
        {
            return MemberInfo.GetValue<T>(ParentObject);
        }

        /// <summary>
        /// Fetches the value of the current member
        /// </summary>
        /// <returns>The value if fetched else null</returns>
        public object GetValue()
        {
            return MemberInfo.GetValue(ParentObject);
        }

        /// <summary>
        /// Sets the value of the member
        /// </summary>
        /// <param name="value">The value which is to be set</param>
        public void SetValue(object value)
        {
            MemberInfo.SetValue(ParentObject, value);
        }

        /// <summary>
        /// Returns the immediate children under the member
        /// </summary>
        public Member[] GetChildren()
        {
            if (ChildMembers == null) FindChildren();
            return ChildMembers ?? Array.Empty<Member>();
        }

        /// <summary>
        /// Returns all the children under the member
        /// </summary>
        public Member[] GetAllChildren()
        {
            if (ChildMembers == null) FindAllChildren();

            var allMembers = Array.Empty<Member>();
            if (ChildMembers == null || ChildMembers.Length == 0) return allMembers;

            for (int i = 0; i < ChildMembers.Length; i++)
            {
                var child = ChildMembers[i];
                allMembers = allMembers
                                        .Append(child)
                                        .Concat(child.GetAllChildren())
                                        .ToArray();
            }

            return allMembers;
        }

        /// <summary>
        /// Checks whether the current member has an attribute of the given type
        /// </summary>
        /// <typeparam name="T">The type of attribute which is to be searched</typeparam>
        /// <returns>Returns true or false based on if the attribute is present on the member or not</returns>
        public virtual bool HasAttribute<T>() where T : Attribute
        {
            if (!HasAttributes) return false;

            for (int i = 0; i < Attributes.Length; i++)
            {
                //Check if the current attribute is of type T
                var att = Attributes[i];
                if (att == null) continue;
                if (att.GetType().Equals(typeof(T))) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the current member has an attribute of the given type and returns the attribute if found
        /// </summary>
        /// <typeparam name="T">The type of attribute which is to be searched</typeparam>
        /// <param name="attribute">The variable in which the attribute is returned if found</param>
        /// <returns>Returns true or false based on if the attribute exists on the member and also outputs the attribute if found</returns>
        public virtual bool TryGetAttribute<T>(out T attribute) where T : Attribute
        {
            attribute = null;

            if (!HasAttributes) return false;

            for (int i = 0; i < Attributes.Length; i++)
            {
                //Check if the current attribute is of type T
                var att = Attributes[i];
                if (att == null) continue;
                if (att is T typedAtt)
                {
                    attribute = typedAtt;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and assigns all the attributes of the member
        /// </summary>
        public virtual void FindAttributes()
        {
            if (MemberInfo == null)
                Attributes = Instance.GetType().GetCustomAttributes(false).Cast<Attribute>().ToArray();
            else
                Attributes = MemberInfo.GetAttributes();
        }

        /// <summary>
        /// Whether the given child is searchable or not
        /// </summary>
        /// <param name="child">The child which is to be searched</param>
        /// <returns>Returns true or false based on if the child is searchable or not</returns>
        public virtual bool IsSearchableChild(Member child)
        {
            //If the member is a method, 
            if (child.MemberInfo is MethodInfo) return false;

            //If it is a primitive type or an array; continue to the next one
            if (child.MemberType.IsPrimitive || child.MemberType.IsArray) return false;

            //If it is a Unity Component
            if (child.MemberType.IsSubclassOf(typeof(Object))) return false;

            return true;
        }

        /// <summary>
        /// Finds the children of the member
        /// </summary>
        public virtual void FindChildren()
        {
            ChildMembers = Array.Empty<Member>();

            //Fetch all the children 
            FindAttributes();
            var allMembers = Instance.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            //Create an empty list to contain all children 
            for (int i = 0; i < allMembers.Length; i++)
            {
                var memberInfo = allMembers[i];

                //If it is a backingfield go to the next one
                var memberName = memberInfo.Name;
                if (memberName.Contains("k__BackingField")) continue;

                //If it a unity property
                string[] unityMembers = { "gameObject", "transform", "mesh" };
                if (memberInfo.HasAttribute<ObsoleteAttribute>() || unityMembers.Contains(memberName)) continue;

                //Gets the member for the given memberInfo
                var member = GetMember(memberInfo);
                if (member == null) continue;

                //Finds the attributes on the member and adds it to the array
                member.Path = $"{Path}.{(member.MemberInfo is PropertyInfo ? $"<{member.Name}>k__BackingField" : member.Name)}";
                member.FindAttributes();
                ChildMembers = ChildMembers.Append(member).ToArray();
            }
        }

        /// <summary>
        /// Finds all the children of the member
        /// </summary>
        public virtual void FindAllChildren(int maxDepth = 10, int currentDepth = 0, List<object> visitedObjects = null)
        {
            //Check if the object has already been searched
            visitedObjects ??= new();
            if (visitedObjects.Contains(Instance)) return;

            visitedObjects.Add(Instance);

            //Check if the depth has gone past the maxDepth
            if (currentDepth > maxDepth) return;

            //Find all children under this member 
            FindChildren();
            if (ChildMembers == null) return;

            //Loop through all the children
            for (int i = 0; i < ChildMembers.Length; i++)
            {
                var child = ChildMembers[i];

                //Check whether the child is searchable or not
                if (!IsSearchableChild(child)) continue;

                //Find all children under the current child
                child.FindAllChildren(maxDepth, currentDepth + 1, visitedObjects);
            }
        }

        /// <summary>
        /// Returns a member for the given memberInfo
        /// </summary>
        /// <param name="memberInfo">The memberInfo for which a member is to be returned</param>
        /// <returns>Returns the newly created member if handled else null</returns>
        public virtual Member GetMember(MemberInfo memberInfo)
        {
            try
            {
                var memberValue = GetValue();
                memberValue ??= memberInfo;
                return new Member(memberInfo, memberValue, Instance);
            }
            catch { return null; }
        }

        /// <summary>
        /// The member title to be used for the string representation of the method
        /// </summary>
        /// <returns>Returns the formatted member title</returns>
        public virtual string GetMemberTitle()
        {
            return $"{Path} [{MemberType}] ({(HasAttributes ? string.Join(',', Attributes.ToList()) : "No attributes")})";
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
