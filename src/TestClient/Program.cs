using System;
using System.Collections.Generic;
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
            var contentSerializer = serviceProvider.GetService<IContentSerializer>();
            var objectStore = serviceProvider.GetService<IObjectStore>();
            var contentTypeResolver = serviceProvider.GetService<IContentTypeResolver>();
            var objectLoader = serviceProvider.GetService<IObjectLoader>();
            var hashCalculator = serviceProvider.GetService<IHashCalculator>();
            var repository = serviceProvider.GetService<IRepository>();

            //var hashText = "0DE2E8A3EE25CE13C495F233F7D54BC9D9A384A1";//"0D194020D7392882A204E7F2F07D662E296067AD";//"318E1B51F019624B0D6ACBA2D2BDE1DC71A91DF7";
            //var hash = new Hash(hashText);
            //var commit = await repository.CheckoutAsync(hash);
            //Hash parentCommitHash = commit.Hash;
            Hash parentCommitHash = null;

            var commitHash = await LoadDataAndCommit(configuration, contentSerializer, repository, hashCalculator, parentCommitHash, cancellationToken);

            //load a full-object from hash
            var contentTypeName = await objectStore.GetObjectTypeAsync(commitHash);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await objectStore.GetObjectContentAsync(commitHash, contentType);
            var commitContent = objectContent as Commit.CommitContent;
            var loadedCommit = commitContent.ToHashableObject(contentSerializer, objectLoader, hashCalculator) as Commit;
            parentCommitHash = loadedCommit.ParentCommitHashes?.FirstOrDefault();
            if (parentCommitHash != null)
            {
                objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType);
                commitContent = objectContent as Commit.CommitContent;
                var parentCommit = commitContent.ToHashableObject(contentSerializer, objectLoader, hashCalculator) as Commit;
                parentCommitHash = parentCommit.ParentCommitHashes?.FirstOrDefault();
                if (parentCommitHash != null)
                {
                    objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType);
                    commitContent = objectContent as Commit.CommitContent;
                    parentCommit = commitContent.ToHashableObject(contentSerializer, objectLoader, hashCalculator) as Commit;
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

        private static async Task<Hash> LoadDataAndCommit(IConfiguration configuration, IContentSerializer contentSerializer, IRepository repository, IHashCalculator hashCalculator, Hash parentCommitHash, CancellationToken cancellationToken)
        {
            var connectionString = configuration.GetConnectionString("CrewSchedule");
            var planVersionId = "1";

            var data = new Data();
            await data.LoadAsync(connectionString, planVersionId, contentSerializer, hashCalculator, cancellationToken);

            var contents = new List<IHashableObject>();
            contents.AddRange(data.Vessels);
            contents.AddRange(data.Seamen);
            contents.AddRange(data.Assignments);
            contents.AddRange(data.Positions);
            //contents.Add(data.Plan);
            //contents.Add(commit);

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix CAPs on Athina", data.Plan, contents, cancellationToken);
            return commitHash;
        }

        private static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IContentSerializer, ProtobufContentSerializer>()
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
