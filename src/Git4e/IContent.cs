namespace Git4e
{
    public interface IContent
    {
        IHashableObject ToHashableObject(IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator);
    }
}
