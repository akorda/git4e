using System;
using System.IO;

namespace Git4e
{
    public abstract class HashableObject : IHashableObject
    {
        public string Type { get; private set; }
        public IContentSerializer ContentSerializer { get; }

        //public virtual void SerializeContent(IContentSerializer contentSerializer, IHashCalculator hashCalculator, Stream stream)
        //{
        //    this.SerializeContent(stream, contentSerializer, hashCalculator);
        //}

        public abstract void SerializeContent(Stream stream, IHashCalculator hashCalculator);

        public HashableObject(string type, IContentSerializer contentSerializer)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.ContentSerializer = contentSerializer ?? throw new ArgumentNullException(nameof(contentSerializer));
        }

        public virtual byte[] ComputeHash(IHashCalculator hashCalculator)
        {
            using (var stream = new MemoryStream())
            {
                this.SerializeContent(stream, hashCalculator);
                stream.Position = 0;
                return hashCalculator.ComputeHash(stream);
            }
        }
    }
}
