using System;
using System.Collections.Generic;

namespace Git4e
{
    /// <inheritdoc/>
    public class ContentTypeResolver : IContentTypeResolver
    {
        private readonly Dictionary<string, Type> Map = new Dictionary<string, Type>();

        /// <inheritdoc />
        public Type ResolveContentType(string contentTypeName)
        {
            if (this.Map.TryGetValue(contentTypeName, out var contentType))
                return contentType;
            return null;
        }

        /// <summary>
        /// Associates a content type name to a content type.
        /// </summary>
        /// <param name="contentTypeName">The content type name.</param>
        /// <param name="contentType">The associated content type.</param>
        public void RegisterContentType(string contentTypeName, Type contentType)
        {
            this.Map[contentTypeName] = contentType;
        }

        /// <summary>
        /// Removes a content type name from the content type resolver.
        /// </summary>
        /// <param name="contentTypeName">The content type name to remove.</param>
        /// <returns><c>true</c> if the content type name was known to the resolver, <c>false</c> otherwise.</returns>
        public bool UnregisterContentType(string contentTypeName)
        {
            return this.Map.Remove(contentTypeName);
        }
    }
}
