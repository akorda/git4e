using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;

namespace Git4e.Tests
{
    public class BranchesUnitTests : IClassFixture<ServiceProviderFixture>
    {
        [Fact(DisplayName = "You can create the same branch multiple times when there is no initial commit")]
        public async Task AllowSameBranchCreationWhenThereIsNoInitialCommit()
        {
            using (var fixture = new ServiceProviderFixture())
            {
                var serviceProvider = fixture.ServiceProvider;

                var repository = serviceProvider.GetService<IRepository>();
                var cancellationToken = CancellationToken.None;

                var newBranch = "work/test1";
                await repository.CreateBranchAsync(newBranch, true, cancellationToken);
                await repository.CreateBranchAsync(newBranch, true, cancellationToken);
            }
        }

        [Fact(DisplayName = "You can not create the same branch multiple times when there is at least one commit")]
        public async Task DoNotAllowSameBranchCreationWhenThereIsAtLEastOneCommit()
        {
            var branch = "work/test1";

            var ex = await Assert.ThrowsAsync<Git4eException>(async () =>
            {
                using (var fixture = new ServiceProviderFixture())
                {
                    var serviceProvider = fixture.ServiceProvider;

                    var repository = serviceProvider.GetService<IRepository>();
                    var cancellationToken = CancellationToken.None;

                    //1. Create a branch
                    await repository.CreateBranchAsync(branch, true, cancellationToken);

                    //2. Make a Commit
                    var data = new TestRootData(repository)
                    {
                        Id = "1",
                        Name = "Root name"
                    };
                    var root = new LazyHashableObject(data);

                    await repository.CommitAsync("author", new DateTime(2021, 05, 15), "a commit message", root, cancellationToken);

                    //3. Try to create the same branch
                    await repository.CreateBranchAsync(branch, true, cancellationToken);
                }
            });
            Assert.Equal(Git4eErrorCode.Exists, ex.ErrorCode);
        }
    }
}
