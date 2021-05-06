using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IContentSerializer
    {
        Task SerializeContentAsync(Stream stream, string type, object content, CancellationToken cancellationToken = default);
        Task<object> DeserializeContentAsync(Stream stream, Type contentType, CancellationToken cancellationToken = default);
        Task<string> GetObjectTypeAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
