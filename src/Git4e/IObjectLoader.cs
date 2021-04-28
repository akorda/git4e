using System.Threading;
using System.Threading.Tasks;

namespace Git4e
{
    public interface IObjectLoader
    {
        Task<IHashableObject> GetObjectByHashAsync(string hash, CancellationToken cancellationToken = default);
    }
}
