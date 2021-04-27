using System;
using System.IO;
using System.Text.Json;

namespace Git4e
{
    public class JsonContentSerializer : IContentSerializer
    {
        class Header
        {
            public string Type { get; set; }
        }

        public object DeserializeContent(Stream stream, Type contentType)
        {
            byte[] buffer;

            using (var streamReader = new BinaryReader(stream))
                buffer = streamReader.ReadBytes((int)stream.Length);

            var jr = new Utf8JsonReader(buffer);
            var header = JsonSerializer.Deserialize<Header>(ref jr);
            //
            var contentBuffer = new byte[buffer.Length - (int)jr.BytesConsumed];
            Buffer.BlockCopy(buffer, (int)jr.BytesConsumed, contentBuffer, 0, buffer.Length - (int)jr.BytesConsumed);
            jr = new Utf8JsonReader(contentBuffer);
            var content = JsonSerializer.Deserialize(ref jr, contentType);
            return content;
        }

        public void SerializeContent(Stream stream, string type, object content)
        {
            var header = new Header { Type = type };

            var writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(writer, header);
            writer.Flush();

            writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(writer, content);
            writer.Flush();
        }

        public string GetObjectTypeAsync(Stream stream)
        {
            byte[] buffer;

            using (var streamReader = new BinaryReader(stream))
                buffer = streamReader.ReadBytes((int)stream.Length);

            var jr = new Utf8JsonReader(buffer);
            var header = JsonSerializer.Deserialize<Header>(ref jr);
            return header.Type;
        }
    }
}
