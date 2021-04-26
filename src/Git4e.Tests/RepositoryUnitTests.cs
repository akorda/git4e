using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;

namespace Git4e.Tests
{
    public class RepositoryUnitTests : IClassFixture<ServiceProviderFixture>
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public RepositoryUnitTests(ServiceProviderFixture fixture)
        {
            this.ServiceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task Test1()
        {
            var objectStore = this.ServiceProvider.GetService<IObjectStore>();
            var repository = this.ServiceProvider.GetService<IRepository>();
            var cancellationToken = CancellationToken.None;

            var newBranch = "work/test1";
            if (!await objectStore.BranchExistsAsync(newBranch, cancellationToken))
            {
                await repository.CreateBranchAsync(newBranch, true, cancellationToken);
            }
        }
    }
}
