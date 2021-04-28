using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public class JsonContentSerializer : IContentSerializer
    {
        class Header
        {
            public string Type { get; set; }
        }

        public async Task<object> DeserializeContentAsync(Stream stream, Type contentType, CancellationToken cancellationToken = default)
        {
            var header = await JsonSerializer.DeserializeAsync<Header>(stream, cancellationToken: cancellationToken);
            var content = await JsonSerializer.DeserializeAsync(stream, contentType, cancellationToken: cancellationToken);
            return content;
        }

        public async Task SerializeContentAsync(Stream stream, string type, object content, CancellationToken cancellationToken = default)
        {
            var header = new Header { Type = type };

            await JsonSerializer.SerializeAsync(stream, header, cancellationToken: cancellationToken);
            await JsonSerializer.SerializeAsync(stream, content, cancellationToken: cancellationToken);
        }

        public async Task<string> GetObjectTypeAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var header = await JsonSerializer.DeserializeAsync<Header>(stream, cancellationToken: cancellationToken);
            return header.Type;
        }
    }
}
