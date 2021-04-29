using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            [ProtoMember(6)]
            public string RootContentType { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
            {
                var commit = new Commit(hash)
                {
                    Author = this.Author,
                    When = this.When,
                    Message = this.Message,
                    Root = new LazyHashableObject(this.RootHash, this.RootContentType),
                    ParentCommitHashes = this.ParentCommitHashes
                };
                return Task.FromResult(commit as IHashableObject);
            }
        }

        public string Author { get; set; }
        public DateTime When { get; set; }
        public string Message { get; set; }
        public LazyHashableObject Root { get; set; }
        public string[] ParentCommitHashes { get; set; }

        public Commit(string hash = null)
            : base(ContentTypeName, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new CommitContent
            {
                Author = this.Author,
                When = this.When,
                Message = this.Message,
                RootHash = this.Root.Hash,
                RootContentType = this.Root.Type,
                ParentCommitHashes = this.ParentCommitHashes
            };
            return content;
        }

        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            if (this.Root != null)
            {
                var root = await this.Root;
                yield return root;
            }
        }
    }
}
