namespace Git4e
{
    public interface IContentToObjectConverter
    {
        IHashableObject ToObject(object content, IContentSerializer contentSerializer, IObjectLoader objectLoader, IHashCalculator hashCalculator);
    }
}
