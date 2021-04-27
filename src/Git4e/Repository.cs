using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Git4e
{
    public class Repository : IRepository
    {
        public IServiceProvider ServiceProvider { get; }
        public IObjectStore ObjectStore { get; }
        public IContentTypeResolver ContentTypeResolver { get; }
        public IObjectLoader ObjectLoader { get; }

        public Repository(
            IServiceProvider serviceProvider,
            IObjectStore objectStore,
            IContentTypeResolver contentTypeResolver,
            IObjectLoader objectLoader = null
            )
        {
            this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.ObjectStore = objectStore ?? throw new ArgumentNullException(nameof(objectStore));
            this.ContentTypeResolver = contentTypeResolver ?? throw new ArgumentNullException(nameof(contentTypeResolver));
            this.ObjectLoader = objectLoader;
        }

        public string HeadCommitHash { get; private set; }

        public async Task<Commit> CheckoutAsync(string commitHash, CancellationToken cancellationToken = default)
        {
            var contentTypeName = await this.ObjectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            if (contentTypeName != Commit.ContentTypeName)
            {
                throw new Exception("Object with hash ... is not a commit");
            }

            var contentType = this.ContentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await this.ObjectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as Commit.CommitContent;
            var commit = commitContent.ToHashableObject(this.ServiceProvider, this.ObjectLoader) as Commit;
            if (commit != null)
            {
                this.HeadCommitHash = commit.Hash;
            }
            return commit;
        }

        public async Task<string> CommitAsync(string author, DateTime when, string message, IHashableObject root, CancellationToken cancellationToken = default)
        {
            var commit = ActivatorUtilities.CreateInstance<Commit>(this.ServiceProvider);
            commit.Author = author;
            commit.When = when;
            commit.Message = message;
            commit.Root = root;

            if (this.HeadCommitHash != null)
            {
                commit.ParentCommitHashes = new[] { this.HeadCommitHash };
            }

            var contents = root.GetAllChildObjects().Union(new[] { commit, root });
            await this.ObjectStore.SaveObjectsAsync(contents, cancellationToken);
            this.HeadCommitHash = commit.Hash;
            return commit.Hash;
        }
    }
}
