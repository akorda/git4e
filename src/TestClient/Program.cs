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
            var hashToTextConverter = serviceProvider.GetService<IHashToTextConverter>();
            var repository = serviceProvider.GetService<IRepository>();

            var hashText = "0D194020D7392882A204E7F2F07D662E296067AD";//"318E1B51F019624B0D6ACBA2D2BDE1DC71A91DF7";
            var hash = hashToTextConverter.ConvertTextToHash(hashText);
            var commit = await repository.CheckoutAsync(hash);
            byte[] parentCommitHash = commit.Hash;
            //byte[] parentCommitHash = null;

            var commitHash = await LoadDataAndCommit(configuration, contentSerializer, objectStore, hashCalculator, parentCommitHash, cancellationToken);

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

        private static async Task<byte[]> LoadDataAndCommit(IConfiguration configuration, IContentSerializer contentSerializer, IObjectStore objectStore, IHashCalculator hashCalculator, byte[] parentCommitHash, CancellationToken cancellationToken)
        {
            var connectionString = configuration.GetConnectionString("CrewSchedule");
            var planVersionId = "1";

            var data = new Data();
            await data.LoadAsync(connectionString, planVersionId, contentSerializer, hashCalculator, cancellationToken);

            var commit = new Commit(contentSerializer, hashCalculator)
            {
                Author = "akorda",
                CommitDate = DateTime.Now,
                Message = "Fix CAPs on Athina",
                Root = data.Plan
            };
            if (parentCommitHash != null)
            {
                commit.ParentCommitHashes = new[] { parentCommitHash };
            }

            var contents = new List<IHashableObject>();
            contents.AddRange(data.Vessels);
            contents.AddRange(data.Seamen);
            contents.AddRange(data.Assignments);
            contents.AddRange(data.Positions);
            contents.Add(data.Plan);
            contents.Add(commit);

            //todo: use objectStore.AddCommit instead of SaveObjectsAsync
            await objectStore.SaveObjectsAsync(contents, cancellationToken);

            return commit.Hash;
        }

        private static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IContentSerializer, ProtobufContentSerializer>()
                .AddSingleton<IHashCalculator, SHA1HashCalculator>()
                .AddSingleton<IHashToTextConverter, HexHashToTextConverter>()
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
