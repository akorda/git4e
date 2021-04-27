using System;
using System.IO;
using ProtoBuf;

namespace Git4e
{
    public class Commit : HashableObject
    {
        public const string ContentTypeName = "Commit";

        [ProtoContract]
        public class CommitContent : IContent
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
            public byte[][] ParentCommitHashes { get; set; }

            public IHashableObject ToHashableObject(IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator)
            {
                return new Commit(contentSerializer, hashCalculator)
                {
                    Author = this.Author,
                    CommitDate = this.CommitDate,
                    Message = this.Message,
                    Root = objectLoader.GetObjectByHash(this.RootHash).Result,
                    ParentCommitHashes = this.ParentCommitHashes
                };
            }
        }

        public string Author { get; set; }
        public DateTime CommitDate { get; set; }
        public string Message { get; set; }
        public IHashableObject Root { get; set; }
        public byte[][] ParentCommitHashes { get; set; }

        public Commit(IContentSerializer contentSerializer, IHashCalculator hashCalculator)
            : base(ContentTypeName, contentSerializer, hashCalculator)
        {
        }

        public override void SerializeContent(Stream stream)
        {
            var rootHash = this.Root.Hash;
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
