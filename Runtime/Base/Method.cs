using System;
using System.Reflection;

namespace UV.EzyReflection
{
    /// <summary>
    /// Defines a method
    /// </summary>
    [Serializable]
    public class Method : Member
    {
        /// <summary>
        /// Initializes a Method with the specified method information, instance, and parent object
        /// </summary>
        /// <param name="methodInfo">The MethodInfo that provides information about this method</param>
        /// <param name="instance">The instance of the object that this method represents</param>
        /// <param name="parentObject">The parent object of this method</param>
        public Method(MethodInfo methodInfo, object instance, object parentObject) : base(methodInfo, instance, parentObject)
        {
            MemberType = methodInfo.ReturnType;
        }
    }
}