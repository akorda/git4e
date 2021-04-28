using System;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IContent
    {
        Task<IHashableObject> ToHashableObjectAsync(IServiceProvider serviceProvider, IObjectLoader objectLoader, CancellationToken cancellationToken = default);
    }
}
