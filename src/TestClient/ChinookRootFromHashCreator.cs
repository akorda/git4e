using Chinook;
using Git4e;

namespace TestClient
{
    public class ChinookRootFromHashCreator : IRootFromHashCreator
    {
        public LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentType)
        {
            return new LazyLibrary(repository, rootHash);
        }
    }
}
