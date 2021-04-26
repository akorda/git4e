using System;

namespace Git4e
{
    /// <summary>
    /// Specifies that a collection is part of a hashable object content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ContentCollectionAttribute : Attribute
    {
    }
}
