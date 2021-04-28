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

            var hash = "E6DD03C0EF03A314A6B30B70FCEE62F47CF2AD5A";//protobuf
            //var hash = "EB217FB7A50A32C986D24C5A4E8C6F592AE9AB43";//json
            var commit = await repository.CheckoutAsync(hash, cancellationToken);
            string parentCommitHash = commit.Hash;
            //string parentCommitHash = null;

            var commitHash = await LoadDataAndCommit(configuration, serviceProvider, repository, cancellationToken);

            //load a full-object from hash
            var contentTypeName = await objectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await objectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as Commit.CommitContent;
            var loadedCommit = (await commitContent.ToHashableObjectAsync(serviceProvider, objectLoader, cancellationToken)) as Commit;
            parentCommitHash = loadedCommit.ParentCommitHashes?.FirstOrDefault();
            if (parentCommitHash != null)
            {
                objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                commitContent = objectContent as Commit.CommitContent;
                var parentCommit = (await commitContent.ToHashableObjectAsync(serviceProvider, objectLoader, cancellationToken)) as Commit;
                parentCommitHash = parentCommit.ParentCommitHashes?.FirstOrDefault();
                if (parentCommitHash != null)
                {
                    objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                    commitContent = objectContent as Commit.CommitContent;
                    parentCommit = (await commitContent.ToHashableObjectAsync(serviceProvider, objectLoader, cancellationToken)) as Commit;
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
                .AddSingleton<IObjectLoader, ObjectLoader>()
                .AddSingleton<IRepository, Repository>()
                .BuildServiceProvider();
        }

        private static IContentTypeResolver CreateContentTypeResolver()
        {
            var resolver = new ContentTypeResolver();
            resolver.RegisterContentType("Commit", typeof(Commit.CommitContent));
            resolver.RegisterContentType("Plan", typeof(CrewSchedule.Plan.PlanContent));
            resolver.RegisterContentType("Seaman", typeof(CrewSchedule.Seaman.SeamanContent));
            resolver.RegisterContentType("SeamanAssignment", typeof(CrewSchedule.SeamanAssignment.SeamanAssignmentContent));
            resolver.RegisterContentType("Vessel", typeof(CrewSchedule.Vessel.VesselContent));
            resolver.RegisterContentType("VesselPosition", typeof(CrewSchedule.VesselPosition.VesselPositionContent));
            return resolver;
        }
    }
}
