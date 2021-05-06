using System;

namespace Git4e
{
    public interface IContentTypeResolver
    {
        Type ResolveContentType(string contentTypeName);
    }
}
