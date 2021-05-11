using System;
using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IContent
    {
        Task<IHashableObject> ToHashableObjectAsync(string hash, IRepository repository, CancellationToken cancellationToken = default);
    }
}
