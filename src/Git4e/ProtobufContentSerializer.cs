using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace Git4e
{
    public class ProtobufContentSerializer : IContentSerializer
    {
        private static Task WriteHeaderAsync(Stream stream, string type, CancellationToken cancellationToken)
        {
            //todo: use ms.WriteAsync()
            var writer = new BinaryWriter(stream);
            writer.Write(type);
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

        public async Task SerializeContentAsync(Stream stream, string type, object content, CancellationToken cancellationToken = default)
        {
            //byte[] bytes;
            //using (var ms = new MemoryStream())
            {
                await WriteHeaderAsync(stream, type, cancellationToken);
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

        public async Task<object> DeserializeContentAsync(Stream stream, Type contentType, CancellationToken cancellationToken = default)
        {
            //Console.WriteLine($"Content deserialized, Type: {contentType.FullName}");
            var type = await ReadHeaderAsync(stream, cancellationToken);
            var content = Serializer.Deserialize(contentType, stream);
            return content;
        }

        public async Task<string> GetObjectTypeAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var type = await ReadHeaderAsync(stream, cancellationToken);
            return type;
        }
    }
}
