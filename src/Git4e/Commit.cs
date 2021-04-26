using System;
using System.IO;
using ProtoBuf;

namespace Git4e
{
    public class Commit : HashableObject
    {
        [ProtoContract]
        public class CommitContent
        {
            [ProtoMember(1)]
            public string Author { get; set; }
            [ProtoMember(2)]
            public DateTime CommitDate { get; set; }
            [ProtoMember(3)]
            public string Message { get; set; }
            [ProtoMember(4)]
            public byte[] RootHash { get; set; }
            [ProtoMember(5)]
            public string[] ParentCommitHashes { get; set; }
        }

        public string Author { get; set; }
        public DateTime CommitDate { get; set; }
        public string Message { get; set; }
        public IHashableObject Root { get; set; }
        public string[] ParentCommitHashes { get; set; }

        public Commit(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base("Commit", contentSerializer, hashCalculator)
        {
        }

        public override void SerializeContent(Stream stream)
        {
            var rootHash = this.Root.ComputeHash();
            var content = new CommitContent
            {
                Author = this.Author,
                CommitDate = this.CommitDate,
                Message = this.Message,
                RootHash = rootHash,
                ParentCommitHashes = this.ParentCommitHashes
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }
    }
}
