using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public static class ContentSerializerExtensions
    {
        public static async Task<T> DeserializeContentAsync<T>(this IContentSerializer contentSerializer, Stream stream, CancellationToken cancellationToken = default)
        {
            var content = await contentSerializer.DeserializeContentAsync(stream, typeof(T), cancellationToken);
            return (T)content;
        }
    }
}
