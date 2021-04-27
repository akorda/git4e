using System.Threading.Tasks;

namespace Git4e
{
    public interface IObjectLoader
    {
        Task<IHashableObject> GetObjectByHash(string hash);
    }
}
