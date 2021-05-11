namespace Git4e
{
    public interface IRootFromHashCreator
    {
        LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentType);
    }
}
