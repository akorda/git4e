namespace Git4e
{
    /// <summary>
    /// Provides a method to instantiate a <see cref="Git4e.LazyHashableObject"/> instance that will be used
    /// as a root hashable object from an object hash and it's content type name
    /// </summary>
    public class RootFromHashCreator : IRootFromHashCreator
    {
        /// <inheritdoc/>
        public LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentTypeName)
        {
            return new LazyHashableObject(repository, rootHash, rootContentTypeName);
        }
    }
}
