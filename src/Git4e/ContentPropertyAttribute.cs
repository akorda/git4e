using System;

namespace Git4e
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ContentPropertyAttribute : Attribute
    {
    }
}
