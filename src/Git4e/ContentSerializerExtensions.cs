using System.IO;

namespace Git4e
{
    public static class ContentSerializerExtensions
    {
        public static T DeserializeContent<T>(this IContentSerializer contentSerializer, Stream stream)
        {
            var content = contentSerializer.DeserializeContent(stream, typeof(T));
            return (T)content;
        }
    }
}
