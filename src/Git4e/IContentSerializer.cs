using System;
using System.IO;

namespace Git4e
{
    public interface IContentSerializer
    {
        void SerializeContent(Stream stream, string type, object content);
        object DeserializeContent(Stream stream, Type contentType);
        string GetObjectTypeAsync(Stream stream);
    }
}
