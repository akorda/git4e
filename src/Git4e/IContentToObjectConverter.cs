using System;

namespace Git4e
{
    public interface IContentToObjectConverter
    {
        IHashableObject ToObject(object content, IServiceProvider serviceProvider, IContentSerializer contentSerializer, IObjectLoader objectLoader);
    }
}
