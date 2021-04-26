using Chinook;
using Git4e;

namespace TestClient
{
    public class ChinookRootFromHashCreator : IRootFromHashCreator
    {
        public LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentTypeName)
        {
            return new LazyLibrary(repository, rootHash);
        }
    }
}
