namespace Git4e
{
    public class RootFromHashCreator : IRootFromHashCreator
    {
        public LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentType)
        {
            return new LazyHashableObject(repository, rootHash, rootContentType); 
        }
    }
}
