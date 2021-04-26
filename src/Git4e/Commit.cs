using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace Git4e
{
    /// <summary>
    /// Represents a commit to the repository.
    /// </summary>
    public class Commit : HashableObject, ICommit
    {
        /// <summary>
        /// The content type name of the <see cref="Git4e.Commit"/>.
        /// </summary>
        public const string CommitContentTypeName = "Commit";

        [ProtoContract]
        class CommitContent : IContent
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

        /// <inheritdoc/>
        public string Author { get; set; }

        /// <inheritdoc/>
        public DateTime When { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; }

        /// <inheritdoc/>
        public LazyHashableObjectBase Root { get; set; }

        /// <inheritdoc/>
        public string[] ParentCommitHashes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Git4e.Commit"/> class of a repository.
        /// </summary>
        /// <param name="repository">The repository that this commit belongs.</param>
        /// <param name="hash">Optional hash of the commit.</param>
        public Commit(IRepository repository, string hash = null)
            : base(repository, CommitContentTypeName, hash)
        {
        }

        /// <summary>
        /// Gets the associated content type of the <see cref="Git4e.Commit"/> class.
        /// </summary>
        /// <returns>The content <see cref="System.Type"/></returns>
        public static Type GetContentType() => typeof(CommitContent);

        /// <summary>
        /// Provides the content of the commit.
        /// </summary>
        /// <returns>The content of this commit.</returns>
        protected override IContent GetContent()
        {
            var content = new CommitContent
            {
                Author = this.Author,
                When = this.When,
                Message = this.Message,
                RootFullHash = this.Root.FullHash,
                RootContentType = this.Root.ContentTypeName,
                ParentCommitHashes = this.ParentCommitHashes
            };
            return content;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<IHashableObject> GetChildObjects()
        {
            if (this.Root != null)
            {
                var root = await this.Root;
                yield return root;
            }
        }

        /// <inheritdoc/>
        public override string UniqueId => null;
    }
}
