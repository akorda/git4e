using Git4e;

namespace Chinook
{
    /// <summary>
    /// Provides a method to instantiate a <see cref="LazyLibrary"/> that will be the root hashable object of Chinook models.
    /// This instance is created using an object hash. The content type name is not used.
    /// </summary>
    public class DefaultRootFromHashCreator : IRootFromHashCreator
    {
        /// <inheritdoc/>
        public LazyHashableObjectBase CreateRootFromHash(IRepository repository, string rootHash, string rootContentTypeName)
        {
            return new LazyLibrary(repository, rootHash);
        }
    }
}
