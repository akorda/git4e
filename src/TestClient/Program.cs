using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrewSchedule;
using Git4e;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            var configuration = GetConfiguration();
            var serviceProvider = CreateServiceProvider();
            var objectStore = serviceProvider.GetService<IObjectStore>();
            var contentTypeResolver = serviceProvider.GetService<IContentTypeResolver>();
            var objectLoader = serviceProvider.GetService<IObjectLoader>();
            var repository = serviceProvider.GetService<IRepository>();
            var hashCalculator = serviceProvider.GetService<IHashCalculator>();
            var contentSerializer = serviceProvider.GetService<IContentSerializer>();

            Globals.ServiceProvider = serviceProvider;
            Globals.HashCalculator = hashCalculator;
            Globals.ContentSerializer = contentSerializer;
            Globals.ObjectStore = objectStore;
            Globals.ObjectLoader = objectLoader;
            Globals.ContentTypeResolver = contentTypeResolver;

            var hash = "EDB0761DAEAFBBD53A42849AAE25BA34AF15E041";//protobuf
            //var hash = "EB217FB7A50A32C986D24C5A4E8C6F592AE9AB43";//json
            var commit = await repository.CheckoutAsync(hash, cancellationToken);
            string parentCommitHash = commit.Hash;
            //Console.WriteLine($"Author: {commit.Author}");
            //var rootHash = commit.Root.Hash;
            //var plan = (await commit.Root) as Plan;
            //var vessel = plan.Vessels.FirstOrDefault();
            ////var v = await vessel.Value;

            //string parentCommitHash = null;

            var commitHash = await LoadDataAndCommit(configuration, serviceProvider, repository, cancellationToken);

            //load a full-object from hash
            var contentTypeName = await objectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await objectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as Commit.CommitContent;
            var loadedCommit = (await commitContent.ToHashableObjectAsync(commitHash, serviceProvider, cancellationToken)) as Commit;
            parentCommitHash = loadedCommit.ParentCommitHashes?.FirstOrDefault();
            if (parentCommitHash != null)
            {
                objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                commitContent = objectContent as Commit.CommitContent;
                var parentCommit = (await commitContent.ToHashableObjectAsync(commitHash, serviceProvider, cancellationToken)) as Commit;
                parentCommitHash = parentCommit.ParentCommitHashes?.FirstOrDefault();
                if (parentCommitHash != null)
                {
                    objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                    commitContent = objectContent as Commit.CommitContent;
                    parentCommit = (await commitContent.ToHashableObjectAsync(commitHash, serviceProvider, cancellationToken)) as Commit;
                }
            }
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.user.json", optional: true)
                .Build();
        }

        private static async Task<string> LoadDataAndCommit(IConfiguration configuration, IServiceProvider serviceProvider, IRepository repository, CancellationToken cancellationToken = default)
        {
            var connectionString = configuration.GetConnectionString("CrewSchedule");
            var planVersionId = "1";

            var data = new Data();
            await data.LoadAsync(connectionString, planVersionId, serviceProvider, cancellationToken);

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix CAPs on Athina", data.Plan, cancellationToken);
            return commitHash;
        }

        private static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IContentSerializer, ProtobufContentSerializer>()
                //.AddSingleton<IContentSerializer, JsonContentSerializer>()
                .AddSingleton<IHashCalculator, SHA1HashCalculator>()
                .AddSingleton<IContentTypeResolver>(CreateContentTypeResolver())
                .AddSingleton<PhysicalFilesObjectStoreOptions>()
                .AddSingleton<IObjectStore, PhysicalFilesObjectStore>()
                //.AddSingleton<IObjectStore, InMemoryObjectStore>()
                .AddSingleton<IObjectLoader, ObjectLoader>()
                .AddSingleton<IRepository, Repository>()
                .BuildServiceProvider();
        }

        private static IContentTypeResolver CreateContentTypeResolver()
        {
            var resolver = new ContentTypeResolver();
            resolver.RegisterContentType(Commit.ContentTypeName, typeof(Commit.CommitContent));
            resolver.RegisterContentType(Plan.PlanContentType, typeof(Plan.PlanContent));
            resolver.RegisterContentType(Seaman.SeamanContentType, typeof(Seaman.SeamanContent));
            resolver.RegisterContentType(SeamanAssignment.SeamanAssignmentContentType, typeof(SeamanAssignment.SeamanAssignmentContent));
            resolver.RegisterContentType(Vessel.VesselContentType, typeof(Vessel.VesselContent));
            resolver.RegisterContentType(VesselPosition.VesselPositionContentType, typeof(VesselPosition.VesselPositionContent));
            return resolver;
        }
    }
}
