using System;
using System.Collections.Generic;

namespace Git4e
{
    public class ContentTypeResolver : IContentTypeResolver
    {
        private Dictionary<string, Type> Map = new Dictionary<string, Type>();

        public Type ResolveContentType(string contentTypeName)
        {
            if (this.Map.TryGetValue(contentTypeName, out var contentType))
                return contentType;
            return null;
        }

        public void RegisterContentType(string contentTypeName, Type contentType)
        {
            this.Map[contentTypeName] = contentType;
        }

        public bool UnRegisterContentType(string contentTypeName)
        {
            return this.Map.Remove(contentTypeName);
        }
    }
}
