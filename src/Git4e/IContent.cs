namespace Git4e
{
    public interface IContent
    {
        object ToObject(IContentSerializer contentSerializer, IObjectLoader objectLoader);
    }
}
