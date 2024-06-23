using UnityEngine;

namespace UV.EzyReflection
{
    /// <summary>
    /// Represents a tree structure of members starting from a root object.
    /// </summary>
    public class MemberTree
    {
        /// <summary>
        /// Initializes a MemberTree class with the specified root member object.
        /// </summary>
        /// <param name="rootMemberObject">The root object from which the member tree will be constructed.</param>
        public MemberTree(object rootMemberObject)
        {
            RootMember = new Member(rootMemberObject);
            RootMember.FindAllChildren();
        }

        /// <summary>
        /// The root member of the member tree.
        /// </summary>
        [field: SerializeField] public Member RootMember { get; private set; }

        public override string ToString()
        {
            return $"Member Tree :\n{RootMember}";
        }
    }
}
