using System;

namespace Git4e
{
    /// <summary>
    /// Provides a method to resolve a content type by it's name.
    /// </summary>
    public interface IContentTypeResolver
    {
        /// <summary>
        /// Resolves the content type by it's name.
        /// </summary>
        /// <param name="contentTypeName">The content type name.</param>
        /// <returns>The <see cref="System.Type"/> of the content type, or <c>null</c> if the content type name is unknown.</returns>
        Type ResolveContentType(string contentTypeName);
    }
}
