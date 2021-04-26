using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace Git4e
{
    /// <summary>
    /// Provides methods for content serialization and deserialization using
    /// the protobuf protocol.
    /// </summary>
    public class ProtobufContentSerializer : IContentSerializer
    {
        private static Task WriteHeaderAsync(Stream stream, string contentTypeName, CancellationToken cancellationToken)
        {
            //todo: use ms.WriteAsync()
            var writer = new BinaryWriter(stream);
            writer.Write(contentTypeName);
            writer.Flush();
            return Task.CompletedTask;
        }

        private static Task<string> ReadHeaderAsync(Stream stream, CancellationToken cancellationToken)
        {
            //todo: use ms.ReadAsync()
            var reader = new BinaryReader(stream);
            var type = reader.ReadString();
            return Task.FromResult(type);
        }

        /// <inheritdoc/>
        public async Task SerializeContentAsync(Stream stream, string contentTypeName, object content, CancellationToken cancellationToken = default)
        {
            //byte[] bytes;
            //using (var ms = new MemoryStream())
            {
                await WriteHeaderAsync(stream, contentTypeName, cancellationToken);
                Serializer.Serialize(stream, content);

                //bytes = ms.ToArray();
            }

            //using (var outStream = new MemoryStream())
            //{
            //    using (var zs = new GZipStream(outStream, CompressionMode.Compress))
            //    using (var inStream = new MemoryStream(bytes))
            //        inStream.CopyTo(zs);

            //    bytes = outStream.ToArray();
            //}

            //return bytes;
        }

        /// <inheritdoc/>
        public async Task<IContent> DeserializeContentAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            //Console.WriteLine($"Content deserialized, Type: {contentType.FullName}");
            await ReadHeaderAsync(stream, cancellationToken);
            var content = Serializer.Deserialize(type, stream) as IContent;
            return content;
        }

        /// <inheritdoc/>
        public async Task<string> GetContentTypeNameAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var contentTypeName = await ReadHeaderAsync(stream, cancellationToken);
            return contentTypeName;
        }
    }
}
