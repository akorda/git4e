using System;
using System.Collections.Generic;
using System.IO;
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
            var commitsComparer = serviceProvider.GetService<ICommitsComparer>();
            var commitsComparerVisitor = serviceProvider.GetService<ICommitsComparerVisitor>();

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

            string commitHash = null;
            //commitHash = await UseCase0(configuration, objectStore, repository, cancellationToken);
            //commitHash = await CreateNewBranch(configuration, objectStore, repository, cancellationToken);
            //commitHash = await UseCase1(configuration, serviceProvider, repository, cancellationToken);
            //commitHash = await UseCase2(configuration, serviceProvider, objectStore, repository, cancellationToken);

            await ShowRepoLog(repository, cancellationToken);
            await Compare2LastCommits(repository, commitsComparer, commitsComparerVisitor, cancellationToken);

            //load a full-object from hash
            var contentTypeName = await objectStore.GetObjectTypeAsync(commitHash, cancellationToken);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var objectContent = await objectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var commitContent = objectContent as Commit.CommitContent;
            var loadedCommit = (await commitContent.ToHashableObjectAsync(commitHash, repository, cancellationToken)) as Commit;
            var parentCommitHash = loadedCommit.ParentCommitHashes?.FirstOrDefault();
            if (parentCommitHash != null)
            {
                objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                commitContent = objectContent as Commit.CommitContent;
                var parentCommit = (await commitContent.ToHashableObjectAsync(commitHash, repository, cancellationToken)) as Commit;
                parentCommitHash = parentCommit.ParentCommitHashes?.FirstOrDefault();
                if (parentCommitHash != null)
                {
                    objectContent = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                    commitContent = objectContent as Commit.CommitContent;
                    parentCommit = (await commitContent.ToHashableObjectAsync(commitHash, repository, cancellationToken)) as Commit;
                }
            }
        }

        private static async Task ShowRepoLog(IRepository repository, CancellationToken cancellationToken)
        {
            await repository.CheckoutAsync("main", cancellationToken);
            await foreach (var commit in repository.GetCommitHistoryAsync(cancellationToken))
            {
                ShowCommitLog(commit);
            }
        }

        private static void ShowCommitLog(ICommit commit)
        {
            Console.WriteLine($"commit {commit.Hash}");
            Console.WriteLine($"Author: {commit.Author}");
            Console.WriteLine($"Date:   {commit.When}");
            Console.WriteLine();

            var messageLines = ("" + commit.Message).Split(Environment.NewLine).AsEnumerable();
            if (!messageLines.Any())
                return;

            Console.WriteLine("\t" + messageLines.First());
            Console.WriteLine();
            messageLines = messageLines.Skip(1);
            if (!messageLines.Any())
                return;

            foreach (var line in messageLines)
            {
                Console.WriteLine("\t" + line);
            }
            Console.WriteLine();
        }

        private static async Task Compare2LastCommits(IRepository repository, ICommitsComparer commitsComparer, ICommitsComparerVisitor commitsComparerVisitor, CancellationToken cancellationToken)
        {
            var branch = "main";
            var lastCommit = await repository.CheckoutAsync(branch, cancellationToken);
            var previousCommit = await repository.GetParentCommitAsync(lastCommit, 1, cancellationToken);

            await commitsComparer.CompareCommits(lastCommit, previousCommit, commitsComparerVisitor, cancellationToken);
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

            var branch = "main";
            var commit = await repository.CheckoutAsync(branch, cancellationToken);
            if (false && commit != null)
            {
                var library = commit.Root.GetValue<Library>();
                foreach (var lazyArtist in library.Artists)
                {
                    var artistId = lazyArtist.ArtistId;
                    var artistName = lazyArtist.Name;

                    var artist = lazyArtist.GetValue<Artist>();
                    foreach (var lazyAlbum in artist.Albums)
                    {
                        var albumId = lazyAlbum.AlbumId;

                        var album = lazyAlbum.GetValue<Album>();
                        foreach (var lazyTrack in album.Tracks)
                        {
                            var trackId = lazyTrack.TrackId;
                            var genreId = lazyTrack.GenreId;
                            var mediaTypeId = lazyTrack.MediaTypeId;

                            var track = lazyTrack.GetValue<Track>();
                        }
                    }
                }
            }

            var data = new Chinook.DataLoader();
            await data.LoadDataAsync(connectionString, repository, cancellationToken);

            var message = @"This is a multiline commit message
Lorem Ipsum is simply dummy text of the printing and typesetting 
industry. Lorem Ipsum has been the industry's standard dummy 
text ever since the 1500s, when an unknown printer took a galley 
of type and scrambled it to make a type specimen book. 

- It has survived not only five centuries, 
- but also the leap into electronic typesetting, 
- remaining essentially unchanged.";
            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, message, data.Library, cancellationToken);
            return commitHash;
        }

        private static async Task<string> CreateNewBranch(IConfiguration configuration, IObjectStore objectStore, IRepository repository, CancellationToken cancellationToken = default)
        {
            var newBranch = "work/test1";
            if (!await objectStore.BranchExistsAsync(newBranch, cancellationToken))
            {
                await repository.CreateBranchAsync(newBranch, true, cancellationToken);
            }

            var connectionString = configuration.GetConnectionString("Chinook");

            var data = new Chinook.DataLoader();
            await data.LoadDataAsync(connectionString, repository, cancellationToken);

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix smth in new branch", data.Library, cancellationToken);
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
            await data.LoadDataAsync(connectionString, planVersionId, serviceProvider, repository, cancellationToken);

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix CAPs on Athina", data.Plan, cancellationToken);
            return commitHash;
        }

        private static async Task<string> UseCase2(IConfiguration configuration, IServiceProvider serviceProvider, IObjectStore objectStore, IRepository repository, CancellationToken cancellationToken = default)
        {
            var hash = await objectStore.ReadHeadAsync(cancellationToken);
            if (hash == null)
                return null;

            var branch = "main";

            var commit = await repository.CheckoutAsync(branch, cancellationToken);
            var lazyPlan = commit.Root as LazyPlan;
            if (lazyPlan.PlanVersionId != "1")
                return null;

            var plan = lazyPlan.GetValue();

            //load ZV and change a vessel property
            var lazyVessel = plan.Vessels.FirstOrDefault(v => v.VesselCode == "ZV");
            lazyVessel.GetValue().Name += "I";
            plan.MarkAsDirty();

            var planHash = plan.FullHash;

            //load UU and change several positions
            lazyVessel = plan.Vessels.FirstOrDefault(v => v.VesselCode == "UU");
            //await Task.WhenAll(lazyVessel.FinalValue.Positions.Select(p => p.Value));

            var lazyPositions = lazyVessel.GetValue().Positions.Where(p => p.DutyRankCode == "OS");

            foreach (var lazyPosition in lazyPositions.ToArray())
            {
                var position = lazyPosition.GetValue();

                position.PositionNo++;

                var asns = position.SeamanAssignments.Where(asn => asn.SeamanCode == "120238").ToArray();
                foreach (var asn in asns)
                {
                    asn.GetValue().StartOverlap--;
                }
            }
            lazyVessel.MarkAsDirty();
            plan.MarkAsDirty();

            planHash = plan.FullHash;

            lazyPlan = new LazyPlan(plan);
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
                .AddSingleton<PhysicalFilesObjectStoreOptions>(_ =>
                {
                    var options = new PhysicalFilesObjectStoreOptions();
                    var path = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    options.RootDirectory = Path.Combine(path, PhysicalFilesObjectStoreOptions.DefaultRootDirectoryName);
                    return options;
                })
                .AddSingleton<IObjectStore, PhysicalFilesObjectStore>()
                .AddSingleton<IObjectLoader, ObjectLoader>()
                .AddSingleton<ICommitsComparerVisitor, ConsoleCommitsComparerVisitor>()
                .AddSingleton<IRootFromHashCreator, ChinookRootFromHashCreator>()
                .AddSingleton<ICommitsComparer, Chinook.CommitsComparer>()
                //.AddSingleton<IRootFromHashCreator, CrewScheduleRootFromHashCreator>()
                //.AddSingleton<ICommitsComparer, CrewSchedule.CommitsComparer>()
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
