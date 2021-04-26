using System.Threading.Tasks;

namespace Git4e
{
    public interface IObjectLoader
    {
        Task<object> GetObjectByHash(byte[] hash);
    }
}
