using System;
using System.Collections.Generic;
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
            var hashToTextConverter = serviceProvider.GetService<IHashToTextConverter>();
            var contentTypeResolver = serviceProvider.GetService<IContentTypeResolver>();
            var objectLoader = serviceProvider.GetService<IObjectLoader>();
            var hashCalculator = serviceProvider.GetService<IHashCalculator>();

            await LoadAndCommit(configuration, contentSerializer, objectStore, hashCalculator, cancellationToken);

            //load a full-object from hash
            var hashText = "9616A3655A008A4429E6135AC2C6932C071B97CB";
            var hash = hashToTextConverter.ConvertTextToHash(hashText);
            var contentTypeName = await objectStore.GetObjectTypeAsync(hash);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var vp = await objectStore.GetObjectContentAsync(hash, contentType);
            var content = vp as IContent;

            var obj = content.ToHashableObject(contentSerializer, objectLoader, hashCalculator);
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.user.json", optional: true)
                .Build();
        }

        private static async Task LoadAndCommit(IConfiguration configuration, IContentSerializer contentSerializer, IObjectStore objectStore, IHashCalculator hashCalculator, CancellationToken cancellationToken)
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

            var contents = new List<IHashableObject>();
            contents.AddRange(data.Vessels);
            contents.AddRange(data.Seamen);
            contents.AddRange(data.Assignments);
            contents.AddRange(data.Positions);
            contents.Add(data.Plan);
            contents.Add(commit);

            //todo: use objectStore.AddCommit instead of SaveObjectsAsync
            await objectStore.SaveObjectsAsync(contents, cancellationToken);
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
