using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
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
            public DateTime When { get; set; }
            [ProtoMember(3)]
            public string Message { get; set; }
            [ProtoMember(4)]
            public string RootHash { get; set; }
            [ProtoMember(5)]
            public string[] ParentCommitHashes { get; set; }

            public IHashableObject ToHashableObject(IServiceProvider serviceProvider, IObjectLoader objectLoader)
            {
                var commit = ActivatorUtilities.CreateInstance<Commit>(serviceProvider);
                commit.Author = this.Author;
                commit.When = this.When;
                commit.Message = this.Message;
                commit.Root = objectLoader.GetObjectByHash(this.RootHash).Result;
                commit.ParentCommitHashes = this.ParentCommitHashes;
                return commit;
            }
        }

        public string Author { get; set; }
        public DateTime When { get; set; }
        public string Message { get; set; }
        public IHashableObject Root { get; set; }
        public string[] ParentCommitHashes { get; set; }

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
                When = this.When,
                Message = this.Message,
                RootHash = rootHash,
                ParentCommitHashes = this.ParentCommitHashes
            };
            this.ContentSerializer.SerializeContent(stream, this.Type, content);
        }

        public override IEnumerable<IHashableObject> ChildObjects
        {
            get
            {
                if (this.Root != null)
                    yield return this.Root;
            }
        }
    }
}
