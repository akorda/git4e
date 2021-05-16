using System;
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
            commitHash = await UseCase0(configuration, objectStore, repository, cancellationToken);
            await Compare2LastCommits(repository, cancellationToken);
            //commitHash = await CreateNewBranch(configuration, objectStore, repository, cancellationToken);
            //commitHash = await UseCase1(configuration, serviceProvider, repository, cancellationToken);
            //commitHash = await UseCase2(configuration, serviceProvider, objectStore, repository, cancellationToken);

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

        private static async Task Compare2LastCommits(IRepository repository, CancellationToken cancellationToken)
        {
            var branch = "main";
            var lastCommit = await repository.CheckoutAsync(branch, cancellationToken);

            var parentCommitHash = lastCommit.ParentCommitHashes?.FirstOrDefault();
            var contentTypeName = await repository.ObjectStore.GetObjectTypeAsync(lastCommit.Hash, cancellationToken);
            var contentType = repository.ContentTypeResolver.ResolveContentType(contentTypeName);
            var commitContent = (await repository.ObjectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken)) as Commit.CommitContent;
            var previousCommit = (await commitContent.ToHashableObjectAsync(parentCommitHash, repository, cancellationToken)) as Commit;

            await Compare2Commits(lastCommit, previousCommit, cancellationToken);
        }

        private static async Task Compare2Commits(ICommit commit, Commit otherCommit, CancellationToken cancellationToken)
        {
            if (commit.Root.Hash == otherCommit.Root.Hash)
            {
                Console.WriteLine($"Commit '{commit.Hash}' and commit '{otherCommit.Hash}' have the same Root");
                return;
            }

            var lib1 = commit.Root.GetValue<Library>();
            var lib2 = otherCommit.Root.GetValue<Library>();

            //find artists in lib1 but not in lib2 and updated artists
            foreach (var la1 in lib1.Artists)
            {
                var la2 = lib2.Artists.FirstOrDefault(a => a.ArtistId == la1.ArtistId);
                if (la2 == null)
                {
                    Console.WriteLine($"Artist '{la1.Name}' ({la1.ArtistId}) created at commit '{commit.Hash}'");
                    continue;
                }

                if (la1.Hash != la2.Hash)
                {
                    Console.WriteLine($"Artist '{la1.Name}' ({la1.ArtistId}) updated at commit '{commit.Hash}'");
                    await CompareArtists(commit, la1.GetValue<Artist>(), la2.GetValue<Artist>(), cancellationToken);
                    continue;
                }
            }

            foreach (var la2 in lib2.Artists)
            {
                var la1 = lib1.Artists.FirstOrDefault(a => a.ArtistId == la2.ArtistId);
                if (la1 == null)
                {
                    Console.WriteLine($"Artist '{la2.Name}' ({la2.ArtistId}) deleted at commit '{commit.Hash}'");
                    continue;
                }
            }
        }

        private static async Task CompareArtists(ICommit commit, Artist artist1, Artist artist2, CancellationToken cancellationToken)
        {
            //compare simple properties. Their Ids are the same

            if (!string.Equals(artist1.Name, artist2.Name))
            {
                Console.WriteLine($"{nameof(Artist.Name)}: '{artist1.Name}' <> '{artist2.Name}'");
            }

            //compare collections
            //find artists in lib1 but not in lib2 and updated artists
            foreach (var al1 in artist1.Albums)
            {
                var al2 = artist2.Albums.FirstOrDefault(a => a.AlbumId == al1.AlbumId);
                if (al2 == null)
                {
                    Console.WriteLine($"Album '{al1.AlbumId}' ({al1.AlbumId}) created at commit '{commit.Hash}'");
                    continue;
                }

                if (al1.Hash != al2.Hash)
                {
                    Console.WriteLine($"Album '{al1.AlbumId}' ({al1.AlbumId}) updated at commit '{commit.Hash}'");
                    await CompareAlbums(commit, al1.GetValue<Album>(), al2.GetValue<Album>(), cancellationToken);
                    continue;
                }
            }

            foreach (var al2 in artist2.Albums)
            {
                var al1 = artist1.Albums.FirstOrDefault(a => a.AlbumId == al2.AlbumId);
                if (al1 == null)
                {
                    Console.WriteLine($"Album '{al2.AlbumId}' deleted at commit '{commit.Hash}'");
                    continue;
                }
            }
        }

        private static async Task CompareAlbums(ICommit commit, Album album1, Album album2, CancellationToken cancellationToken)
        {
            //compare simple properties. Their Ids are the same

            if (!string.Equals(album1.Title, album2.Title))
            {
                Console.WriteLine($"{nameof(Album.Title)}: '{album1.Title}' <> '{album2.Title}'");
            }

            //compare collections
            //find artists in lib1 but not in lib2 and updated artists
            foreach (var tr1 in album1.Tracks)
            {
                var tr2 = album2.Tracks.FirstOrDefault(a => a.TrackId == tr1.TrackId);
                if (tr2 == null)
                {
                    Console.WriteLine($"Album '{tr1.TrackId}' ({tr1.TrackId}) created at commit '{commit.Hash}'");
                    continue;
                }

                if (tr1.Hash != tr2.Hash)
                {
                    Console.WriteLine($"Album '{tr1.TrackId}' ({tr1.TrackId}) updated at commit '{commit.Hash}'");
                    await CompareTracks(commit, tr1.GetValue<Track>(), tr2.GetValue<Track>(), cancellationToken);
                    continue;
                }
            }

            foreach (var tr2 in album2.Tracks)
            {
                var tr1 = album1.Tracks.FirstOrDefault(a => a.TrackId == tr2.TrackId);
                if (tr1 == null)
                {
                    Console.WriteLine($"Track '{tr2.TrackId}' deleted at commit '{commit.Hash}'");
                    continue;
                }
            }
        }

        private static Task CompareTracks(ICommit commit, Track track1, Track track2, CancellationToken cancellationToken)
        {
            //compare simple properties. Their Ids are the same

            if (!string.Equals(track1.Name, track2.Name))
            {
                Console.WriteLine($"{nameof(Track.Name)}: '{track1.Name}' <> '{track2.Name}'");
            }
            if (!string.Equals(track1.Composer, track2.Composer))
            {
                Console.WriteLine($"{nameof(Track.Composer)}: '{track1.Composer}' <> '{track2.Composer}'");
            }
            if (track1.Milliseconds != track2.Milliseconds)
            {
                Console.WriteLine($"{nameof(Track.Milliseconds)}: '{track1.Milliseconds}' <> '{track2.Milliseconds}'");
            }
            if (track1.Bytes != track2.Bytes)
            {
                Console.WriteLine($"{nameof(Track.Bytes)}: '{track1.Bytes}' <> '{track2.Bytes}'");
            }
            if (track1.UnitPrice != track2.UnitPrice)
            {
                Console.WriteLine($"{nameof(Track.UnitPrice)}: '{track1.UnitPrice}' <> '{track2.UnitPrice}'");
            }
            if (!string.Equals(track1.Genre.UniqueId, track2.Genre.UniqueId))
            {
                Console.WriteLine($"{nameof(Track.Genre)}: '{track1.Genre.UniqueId}' <> '{track2.Genre.UniqueId}'");
            }
            if (!string.Equals(track1.MediaType.UniqueId, track2.MediaType.UniqueId))
            {
                Console.WriteLine($"{nameof(Track.MediaType)}: '{track1.MediaType.UniqueId}' <> '{track2.MediaType.UniqueId}'");
            }
            return Task.CompletedTask;
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

            var commitHash = await repository.CommitAsync("akorda", DateTime.Now, "Fix albums", data.Library, cancellationToken);
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
            //load ZV and change a vessel property
            var commit = await repository.CheckoutAsync(branch, cancellationToken);
            var root = commit.Root as LazyPlan;
            if (root.PlanVersionId != "1")
                return null;

            //var plan = (await commit.Root) as Plan;
            var plan = commit.Root.GetValue<Plan>();

            var lazyVessel = plan.Vessels.FirstOrDefault(v => v.VesselCode == "ZV");
            lazyVessel.GetValue<Vessel>().Name += "I";
            plan.MarkAsDirty();

            var planHash = plan.FullHash;

            //load UU and change several positions
            lazyVessel = plan.Vessels.FirstOrDefault(v => v.VesselCode == "UU");
            //await Task.WhenAll(lazyVessel.FinalValue.Positions.Select(p => p.Value));

            var lazyPositions = lazyVessel.GetValue<Vessel>().Positions.Where(p => p.DutyRankCode == "OS");

            foreach (var lazyPosition in lazyPositions.ToArray())
            {
                lazyPosition.GetValue<VesselPosition>().PositionNo++;

                var asns = lazyPosition.GetValue<VesselPosition>().SeamanAssignments.Where(asn => asn.SeamanCode == "120238").ToArray();
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
                .AddSingleton<PhysicalFilesObjectStoreOptions>(_ =>
                {
                    var options = new PhysicalFilesObjectStoreOptions();
                    var path = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    options.RootDirectory = Path.Combine(path, PhysicalFilesObjectStoreOptions.DefaultRootDirectoryName);
                    return options;
                })
                .AddSingleton<IObjectStore, PhysicalFilesObjectStore>()
                .AddSingleton<IObjectLoader, ObjectLoader>()
                .AddSingleton<IRootFromHashCreator, ChinookRootFromHashCreator>()
                //.AddSingleton<IRootFromHashCreator, CrewScheduleRootFromHashCreator>()
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
