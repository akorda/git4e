using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Git4e.Tests
{
    public class ServiceProviderFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }
        string TempRootDir { get; set; }

        public ServiceProviderFixture()
        {
            this.TempRootDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(this.TempRootDir);

            this.ServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IContentSerializer, ProtobufContentSerializer>()
                .AddSingleton<IHashCalculator, SHA1HashCalculator>()
                .AddSingleton<IContentTypeResolver>(CreateContentTypeResolver())
                .AddSingleton<PhysicalFilesObjectStoreOptions>(_ =>
                {
                    var options = new PhysicalFilesObjectStoreOptions
                    {
                        RootDirectory = Path.Combine(this.TempRootDir, PhysicalFilesObjectStoreOptions.DefaultRootDirectoryName)
                    };
                    return options;
                })
                .AddSingleton<IObjectStore, PhysicalFilesObjectStore>()
                .AddSingleton<IObjectLoader, ObjectLoader>()
                .AddSingleton<IRootFromHashCreator, RootFromHashCreator>()
                .AddSingleton<IRepository, Repository>()
                .BuildServiceProvider();
        }

        private static IContentTypeResolver CreateContentTypeResolver()
        {
            var resolver = new ContentTypeResolver();

            resolver.RegisterContentType(Commit.ContentTypeName, typeof(Commit.CommitContent));
            return resolver;
        }

        public void Dispose()
        {
            Directory.Delete(this.TempRootDir, true);
        }
    }
}
