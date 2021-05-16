using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace Git4e
{
    public class Commit : HashableObject, ICommit
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
            public string RootFullHash { get; set; }
            [ProtoMember(5)]
            public string RootContentType { get; set; }
            [ProtoMember(6)]
            public string[] ParentCommitHashes { get; set; }

            public Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default)
            {
                IRootFromHashCreator rootCreator = repository.RootFromHashCreator ?? new RootFromHashCreator();
                var commit = new Commit(repository, hash)
                {
                    Author = this.Author,
                    When = this.When,
                    Message = this.Message,
                    Root = rootCreator.CreateRootFromHash(repository, this.RootFullHash, this.RootContentType),
                    ParentCommitHashes = this.ParentCommitHashes
                };
                return Task.FromResult(commit as IHashableObject);
            }
        }

        public string Author { get; set; }
        public DateTime When { get; set; }
        public string Message { get; set; }
        public LazyHashableObjectBase Root { get; set; }
        public string[] ParentCommitHashes { get; set; }

        public Commit(IRepository repository, string hash = null)
            : base(repository, ContentTypeName, hash)
        {
        }

        protected override object GetContent()
        {
            var content = new CommitContent
            {
                Author = this.Author,
                When = this.When,
                Message = this.Message,
                RootFullHash = this.Root.FullHash,
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

        public override string UniqueId => null;
    }
}
