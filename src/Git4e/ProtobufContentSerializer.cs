using System;
using System.IO;
using ProtoBuf;

namespace Git4e
{
    public class ProtobufContentSerializer : IContentSerializer
    {
        public void SerializeContent(Stream stream, string type, object content)
        {
            //byte[] bytes;
            //using (var ms = new MemoryStream())
            {
                WriteHeader(stream, type);
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

        public object DeserializeContent(Stream stream, Type contentType)
        {
            var type = ReadHeader(stream);
            var content = Serializer.Deserialize(contentType, stream);
            return content;
        }

        private static void WriteHeader(Stream stream, string type)
        {
            //todo: use ms.WriteAsync()
            var writer = new BinaryWriter(stream);
            writer.Write(type);
            writer.Flush();
        }

        private static string ReadHeader(Stream stream)
        {
            //todo: use ms.ReadAsync()
            var reader = new BinaryReader(stream);
            var type = reader.ReadString();
            return type;
        }

        public string GetObjectTypeAsync(Stream stream)
        {
            var type = ReadHeader(stream);
            return type;
        }
    }
}
