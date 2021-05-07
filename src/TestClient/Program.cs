using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chinook;
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

            //crew schedule
            //Globals.RootFromHashCreator = new Func<string, string, LazyHashableObject>((rootHash, rootContentType) =>
            //{
            //    if (rootHash.IndexOf("|") != -1)
            //        return new LazyPlan(rootHash);
            //    else
            //        return new LazyHashableObject(rootHash, rootContentType);
            //});

            //chinook
            Globals.RootFromHashCreator = new Func<string, string, LazyHashableObject>((rootHash, rootContentType) => new LazyLibrary(rootHash));

            //var hash = await objectStore.ReadHeadAsync(cancellationToken);
            //string parentCommitHash;
            //if (hash != null)
            //{
            //    var commit = await repository.CheckoutAsync(hash, cancellationToken);
            //    parentCommitHash = commit.Hash;
            //}

            //Console.WriteLine($"Author: {commit.Author}");
            //var rootHash = commit.Root.Hash;
            //var plan = (await commit.Root) as Plan;
            //var vessel = plan.Vessels.FirstOrDefault();
            ////var v = await vessel.Value;

            //string parentCommitHash = null;

            var commitHash = await UseCase0(configuration, objectStore, repository, cancellationToken);
            //var commitHash = await UseCase1(configuration, serviceProvider, repository, cancellationToken);
            //var commitHash = await UseCase2(configuration, serviceProvider, objectStore, repository, cancellationToken);

            //load a full-object from hash
            var contentTypeName = await objectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await objectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as Commit.CommitContent;
            var loadedCommit = (await commitContent.ToHashableObjectAsync(commitHash, serviceProvider, cancellationToken)) as Commit;
            var parentCommitHash = loadedCommit.ParentCommitHashes?.FirstOrDefault();
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

        private static async Task<string> UseCase0(IConfiguration configuration, IObjectStore objectStore, IRepository repository, CancellationToken cancellationToken = default)
        {
            var connectionString = configuration.GetConnectionString("Chinook");

            var data = new Chinook.DataLoader();
            await data.LoadDataAsync(connectionString, cancellationToken);

            var headHash = await objectStore.ReadHeadAsync(cancellationToken);
            if (headHash != null)
                await repository.CheckoutAsync(headHash, cancellationToken);

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix albums", data.Library, cancellationToken);
            return commitHash;
        }

        /// <summary>
        /// Use Case 1:
        /// 1. Checkout latest repo version
        /// 2. Load all plan data
        /// 3. Commit to verify that the commit includes just the updated plan data
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="repository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<string> UseCase1(IConfiguration configuration, IServiceProvider serviceProvider, IRepository repository, CancellationToken cancellationToken = default)
        {
            var connectionString = configuration.GetConnectionString("CrewSchedule");
            var planVersionId = "1";

            var data = new CrewSchedule.DataLoader();
            await data.LoadDataAsync(connectionString, planVersionId, serviceProvider, cancellationToken);

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix CAPs on Athina", data.Plan, cancellationToken);
            return commitHash;
        }

        private static async Task<string> UseCase2(IConfiguration configuration, IServiceProvider serviceProvider, IObjectStore objectStore, IRepository repository, CancellationToken cancellationToken = default)
        {
            var hash = await objectStore.ReadHeadAsync(cancellationToken);
            if (hash == null) return null;

            //load ZV and change a vessel property
            var commit = await repository.CheckoutAsync(hash, cancellationToken);
            var root = commit.Root as LazyHashableObject<string>;
            if (root.HashIncludeProperty1 != "1")
                return null;

            //var plan = (await commit.Root) as Plan;
            var plan = commit.Root.GetValue<Plan>();

            var lazyVessel = plan.Vessels.FirstOrDefault(v => v.HashIncludeProperty1 == "ZV");
            lazyVessel.GetValue<Vessel>().Name += "I";
            plan.MarkAsDirty();

            var planHash = plan.FullHash;

            //load UU and change several positions
            lazyVessel = plan.Vessels.FirstOrDefault(v => v.HashIncludeProperty1 == "UU");
            //await Task.WhenAll(lazyVessel.FinalValue.Positions.Select(p => p.Value));

            var lazyPositions = lazyVessel.GetValue<Vessel>().Positions.Where(p => p.HashIncludeProperty1 == "OS");

            foreach (var lazyPosition in lazyPositions.ToArray())
            {
                lazyPosition.GetValue<VesselPosition>().PositionNo++;

                var asns = lazyPosition.GetValue<VesselPosition>().SeamanAssignments.Where(asn => asn.HashIncludeProperty2 == "120238").ToArray();
                foreach (var asn in asns)
                {
                    asn.GetValue<SeamanAssignment>().StartOverlap--;
                }
            }
            lazyVessel.MarkAsDirty();
            plan.MarkAsDirty();

            planHash = plan.FullHash;

            var lazyPlan = new LazyPlan(plan);
            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Change Vessel Name", lazyPlan, cancellationToken);
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

            resolver.RegisterContentType(Commit.ContentTypeName, typeof(Commit.CommitContent));

            //Crew Schedule
            resolver.RegisterContentType(Plan.PlanContentType, typeof(Plan.PlanContent));
            resolver.RegisterContentType(Seaman.SeamanContentType, typeof(Seaman.SeamanContent));
            resolver.RegisterContentType(SeamanAssignment.SeamanAssignmentContentType, typeof(SeamanAssignment.SeamanAssignmentContent));
            resolver.RegisterContentType(Vessel.VesselContentType, typeof(Vessel.VesselContent));
            resolver.RegisterContentType(VesselPosition.VesselPositionContentType, typeof(VesselPosition.VesselPositionContent));

            //Chinook
            resolver.RegisterContentType(Library.LibraryContentType, typeof(Library.LibraryContent));
            resolver.RegisterContentType(Artist.ArtistContentType, typeof(Artist.ArtistContent));
            resolver.RegisterContentType(Album.AlbumContentType, typeof(Album.AlbumContent));
            resolver.RegisterContentType(Track.TrackContentType, typeof(Track.TrackContent));
            resolver.RegisterContentType(Genre.GenreContentType, typeof(Genre.GenreContent));
            resolver.RegisterContentType(MediaType.MediaTypeContentType, typeof(MediaType.MediaTypeContent));
            return resolver;
        }
    }
}
