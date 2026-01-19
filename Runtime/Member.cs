using System;
using System.Text;
using System.Linq;
using UnityEngine;
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
        public Member(object instance)
        {
            Instance = instance;
            Name = GetSafeName(Instance);
            Path = Name;
            MemberType = Instance?.GetType();
        }

        /// <summary>
        /// Initializes a new Member with the specified instance and parent object.
        /// </summary>
        /// <param name="instance">The instance of the object that this member represents.</param>
        /// <param name="parentObject">The parent object of this member.</param>
        private Member(object instance, object parentObject) : this(instance)
        {
            ParentObject = parentObject;
        }

        /// <summary>
        /// Initializes a new Member with the specified member information, instance, and parent object.
        /// </summary>
        /// <param name="memberInfo">The MemberInfo that provides information about this member.</param>
        /// <param name="instance">The instance of the object that this member represents.</param>
        /// <param name="parentObject">The parent object of this member.</param>
        public Member(MemberInfo memberInfo, object instance, object parentObject) : this(instance, parentObject)
        {
            MemberInfo = memberInfo;
            Name = memberInfo.Name;

            if (memberInfo is FieldInfo fieldInfo)
                MemberType = fieldInfo.FieldType;

            if (memberInfo is PropertyInfo propertyInfo)
                MemberType = propertyInfo.PropertyType;

            if (memberInfo is MethodInfo methodInfo)
                MemberType = methodInfo.ReturnType;
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
        public T[] GetChildren<T>() where T : Member
        {
            if (ChildMembers == null) return Array.Empty<T>();
            return ChildMembers.Cast<T>().ToArray() ?? Array.Empty<T>();
        }

        /// <summary>
        /// Returns all the children under the member
        /// </summary>
        public T[] GetAllChildren<T>() where T : Member
        {
            var allMembers = Array.Empty<Member>();
            if (ChildMembers == null || ChildMembers.Length == 0) return Array.Empty<T>();

            for (int i = 0; i < ChildMembers.Length; i++)
            {
                var child = ChildMembers[i];
                allMembers = allMembers
                                        .Append(child)
                                        .Concat(child.GetAllChildren<T>())
                                        .ToArray();
            }

            return allMembers.Cast<T>().ToArray();
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
                if (att is T) return true;
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
                if (att is not T typedAtt) continue;

                //If the attribute is found return true and the attribute
                attribute = typedAtt;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds and assigns all the attributes of the member
        /// </summary>
        public virtual void FindAttributes()
        {
            Attributes = Array.Empty<Attribute>();
            if (MemberInfo != null)
                Attributes = Attributes.Concat(MemberInfo.GetAttributes()).ToArray();

            Attributes = Attributes.Concat(Instance.GetType().GetCustomAttributes().Cast<Attribute>()).ToArray();
        }

        /// <summary>
        /// Whether the given child is searchable or not
        /// </summary>
        /// <param name="child">The child which is to be searched</param>
        /// <returns>Returns true or false based on if the child is searchable or not</returns>
        public virtual bool IsSearchableChild(Member child)
        {
            //If the member is a method, 
            if (child.MemberInfo is MethodBase) return false;
            return IsSearchableType(child.MemberType);
        }

        /// <summary>
        /// Whether the given type is searchable or not
        /// </summary>
        /// <param name="type">The type which is to be searched</param>
        /// <param name="unityTypes">Whether Unity types are allowed or not</param>
        /// <returns>Returns true or false based on if the type is searchable or not</returns>
        public virtual bool IsSearchableType(Type type, bool unityTypes = false)
        {
            if (type == null) return false;

            //If it is a primitive type or an array; continue to the next one
            if (type.IsSimpleType()) return false;
            if (type.IsSubclassOf(typeof(Object)) && !unityTypes) return false;
            if (type.Equals(typeof(System.Threading.CancellationToken))) return false;
            if (type.Equals(typeof(System.Threading.CancellationTokenSource))) return false;
            return true;
        }

        /// <summary>
        /// Whether the given member is a valid member 
        /// </summary>
        /// <param name="memberInfo">The memberInfo which is to be checked</param>
        /// <returns>Returns true or false based on whether the given member is a valid member or not</returns>
        public virtual bool IsValidMember(MemberInfo memberInfo)
        {
            if (memberInfo == null) return false;

            var memberName = memberInfo.Name;
            if (memberName.Contains("k__BackingField")) return false;
            if (memberInfo.HasAttribute<ObsoleteAttribute>()) return false;
            return true;
        }

        /// <summary>
        /// Finds the children of the member
        /// </summary>
        public virtual void FindChildren()
        {
            if (Instance == null) return;

            ChildMembers = Array.Empty<Member>();
            var members = new List<Member>();

            // Fetch all the children
            FindAttributes();

            // Get all members including inherited private members
            var currentType = Instance.GetType();
            while (currentType != null)
            {
                if (!IsSearchableType(currentType, true)) break;

                var allMembers = currentType.GetMembers(BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Instance |
                                                        BindingFlags.Static |
                                                        BindingFlags.DeclaredOnly);

                var currentTypeMembers = new List<Member>();

                for (int i = 0; i < allMembers.Length; i++)
                {
                    var memberInfo = allMembers[i];
                    if (!IsValidMember(memberInfo)) continue;

                    // Gets the member for the given memberInfo
                    var member = GetMember(memberInfo);
                    if (member == null) continue;

                    // Finds the attributes on the member and adds it to the array
                    var name = memberInfo is PropertyInfo && !string.IsNullOrEmpty(member?.Name) ? $"<{member.Name}>k__BackingField" : member?.Name;
                    member.Path = $"{Path}.{name}";
                    member.FindAttributes();
                    currentTypeMembers.Add(member);
                }

                currentType = currentType.BaseType;
                members = currentTypeMembers.Concat(members).ToList();
            }

            ChildMembers = members.ToArray();
        }

        /// <summary>
        /// Finds all the children of the member
        /// </summary>
        public virtual void FindAllChildren(int maxDepth = 10, int currentDepth = 0, List<object> visitedObjects = null)
        {
            //Check if the object has already been searched
            if (Instance == null) return;
            visitedObjects ??= new();
            if (visitedObjects.Contains(Instance)) return;
            visitedObjects.Add(Instance);
            if (currentDepth > maxDepth) return;

            //Find all children under this member and loop through them
            FindChildren();
            if (ChildMembers == null) return;
            for (int i = 0; i < ChildMembers.Length; i++)
            {
                var child = ChildMembers[i];
                if (child == null) continue;

                //Check whether the child is searchable or not
                if (!IsSearchableChild(child))
                    continue;

                //Find all children under the current child
                child.FindAllChildren(maxDepth, currentDepth + 1, visitedObjects);
            }
        }

        /// <summary>
        /// Adds the given member as the child of this member
        /// </summary>
        /// <param name="child">The child which is to be added</param>
        public virtual void AddChild(Member child)
        {
            ChildMembers ??= Array.Empty<Member>();
            ChildMembers = ChildMembers.Append(child).ToArray();
        }

        /// <summary>
        /// Returns a member for the given memberInfo
        /// </summary>
        /// <param name="memberInfo">The memberInfo for which a member is to be returned</param>
        /// <returns>Returns the newly created member if handled else null</returns>
        public virtual Member GetMember(MemberInfo memberInfo)
        {
            if (memberInfo is MethodInfo)
                return new Member(memberInfo, memberInfo, Instance);

            try
            {
                object memberValue = null;
                if (Instance is Renderer renderer)
                {
                    if (memberInfo.Name.Equals("material"))
                        memberValue = Application.isPlaying ? renderer.material : renderer.sharedMaterial;

                    if (memberInfo.Name.Equals("materials"))
                        memberValue = Application.isPlaying ? renderer.materials : renderer.sharedMaterials;
                }
                else
                    memberValue = memberInfo.GetValue(Instance);

                memberValue ??= memberInfo;
                return new Member(memberInfo, memberValue, Instance);
            }
            catch
            {
                return new Member(memberInfo, memberInfo, Instance);
            }
        }

        /// <summary>
        /// Finds the member with the given name under this member 
        /// </summary>
        /// <typeparam name="T">The type of members to be searched</typeparam>
        /// <param name="memberName">The name of the member</param>
        /// <param name="searchUnderChildren">Whether the member is to be searched under all the children as well</param>
        /// <returns>Returns the member if found else null</returns>
        public virtual Member FindMember(string memberName, bool searchUnderChildren = false)
        {
            //Look under immediate children
            var children = GetChildren<Member>();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null) continue;

                //If the name matches then return the child 
                var childName = child.Name;
                if (childName == null) continue;
                if (childName.Equals(memberName) || childName.Equals($"<{memberName}>k__BackingField"))
                    return child;
            }

            //Look under child if allowed and required
            if (!searchUnderChildren) return null;
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null) continue;

                //Search it under the child
                var foundMember = child.FindMember(memberName, searchUnderChildren);
                if (foundMember == null) continue;

                //If found then return it
                return foundMember;
            }
            return null;
        }

        /// <summary>
        /// Finds the member with the given name under this member 
        /// </summary>
        /// <typeparam name="T">The type of members to be searched</typeparam>
        /// <param name="memberName">The name of the member</param>
        /// <param name="searchUnderChildren">Whether the member is to be searched under all the children as well</param>
        /// <returns>Returns the member if found else null</returns>
        public virtual T FindMember<T>(string memberName, bool searchUnderChildren = false) where T : Member
        {
            return FindMember(memberName, searchUnderChildren) as T;
        }

        /// <summary>
        /// Finds all the members with the given attribute under this member
        /// </summary>
        /// <typeparam name="T">The type of attribute which is to be searched</typeparam>
        /// <param name="searchUnderChildren">Whether the children are to be searched as well</param>
        /// <returns>Returns a tuple array (Member, T)[] for all the members with the attribute if found else an empty array</returns>
        public virtual (Member, T)[] FindMembersWithAttribute<T>(bool searchUnderChildren = false) where T : Attribute
        {
            //Look under immediate children
            var members = new List<(Member, T)>();
            var children = GetChildren<Member>();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null) continue;
                if (child.TryGetAttribute(out T attribute)) members.Add((child, attribute));
            }

            //Look under child if allowed and required
            if (!searchUnderChildren) return members.ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null) continue;

                //Search it under the child
                var foundMembers = child.FindMembersWithAttribute<T>(searchUnderChildren);
                if (foundMembers == null || foundMembers.Length == 0) continue;

                //If found then return it
                members.AddRange(foundMembers);
            }

            return members.ToArray();
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
            var output = new StringBuilder(GetMemberTitle());
            if (ChildMembers == null || ChildMembers.Length == 0) return output.ToString();

            output.Append(" : ");
            for (int i = 0; i < ChildMembers.Length; i++)
                output.Append($"\n{new string(' ', indent * 2)} - {ChildMembers[i].GetString(indent + 1)}");

            return output.ToString();
        }

        public override string ToString()
        {
            return GetString();
        }
        
        private static string GetSafeName(object instance)
        {
            if (instance == null)
                return "null";

            // Unity objects: never call ToString()
            if (instance is UnityEngine.Object uObj)
                return uObj.name;

            // NetworkBehaviours / engine objects: use type name
            var type = instance.GetType();

            // Avoid overridden ToString() entirely
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
                return type.Name;

            return type.Name;
        }
    }
}
