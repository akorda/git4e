namespace Git4e
{
    /// <summary>
    /// Provides a method to instantiate a root hashable object from an object hash
    /// and it's content type name
    /// </summary>
    public interface IRootFromHashCreator
    {
        /// <summary>
        /// Instantiates a root hashable object from an object hash and it's content type name.
        /// </summary>
        /// <param name="repository">The repository that this hashable object belongs to.</param>
        /// <param name="rootHash">The hash of the root object.</param>
        /// <param name="rootContentTypeName">The content type name of the root object.</param>
        /// <returns>The lazy hashable root object</returns>
        LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentTypeName);
    }
}
