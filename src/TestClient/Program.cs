using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chinook;
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
            var contentTypeName = await objectStore.GetContentTypeNameAsync(commitHash, cancellationToken);
            var contentType = contentTypeResolver.ResolveContentType(contentTypeName);
            var content = await objectStore.GetObjectContentAsync(commitHash, contentType, cancellationToken);
            var loadedCommit = (await content.ToHashableObjectAsync(commitHash, repository, cancellationToken)) as Commit;
            var parentCommitHash = loadedCommit.ParentCommitHashes?.FirstOrDefault();
            if (parentCommitHash != null)
            {
                content = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                var parentCommit = (await content.ToHashableObjectAsync(commitHash, repository, cancellationToken)) as Commit;
                parentCommitHash = parentCommit.ParentCommitHashes?.FirstOrDefault();
                if (parentCommitHash != null)
                {
                    content = await objectStore.GetObjectContentAsync(parentCommitHash, contentType, cancellationToken);
                    parentCommit = (await content.ToHashableObjectAsync(commitHash, repository, cancellationToken)) as Commit;
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

            await commitsComparer.CompareCommitsAsync(lastCommit, previousCommit, commitsComparerVisitor, cancellationToken);
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
                var library = commit.Root.LoadValue<Library>();
                foreach (var lazyArtist in library.Artists)
                {
                    var artistId = lazyArtist.ArtistId;
                    var artistName = lazyArtist.Name;

                    var artist = lazyArtist.LoadValue();
                    foreach (var lazyAlbum in artist.Albums)
                    {
                        var albumId = lazyAlbum.AlbumId;

                        var album = lazyAlbum.LoadValue();
                        foreach (var lazyTrack in album.Tracks)
                        {
                            var trackId = lazyTrack.TrackId;
                            var genreId = lazyTrack.GenreId;
                            var mediaTypeId = lazyTrack.MediaTypeId;

                            var track = lazyTrack.LoadValue();
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
                .AddSingleton<ICommitsComparer, Git4e.CommitsComparer>()
                .AddSingleton<IRootFromHashCreator, ChinookRootFromHashCreator>()
                //.AddSingleton<ICommitsComparer, Chinook.CommitsComparer>()
                .AddSingleton<IRepository, Repository>()
                .BuildServiceProvider();
        }

        private static IContentTypeResolver CreateContentTypeResolver()
        {
            var resolver = new ContentTypeResolver();

            resolver.RegisterContentType(Commit.CommitContentTypeName, Commit.GetContentType());

            //Chinook
            resolver.RegisterContentType(Library.LibraryContentTypeName, Library.GetContentType());
            resolver.RegisterContentType(Artist.ArtistContentTypeName, Artist.GetContentType());
            resolver.RegisterContentType(Album.AlbumContentTypeName, Album.GetContentType());
            resolver.RegisterContentType(Track.TrackContentTypeName, Track.GetContentType());
            resolver.RegisterContentType(Genre.GenreContentTypeName, Genre.GetContentType());
            resolver.RegisterContentType(MediaType.MediaTypeContentTypeName, MediaType.GetContentType());

            return resolver;
        }
    }
}
