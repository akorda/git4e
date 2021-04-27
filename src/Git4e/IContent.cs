using System;

namespace Git4e
{
    public interface IContent
    {
        IHashableObject ToHashableObject(IServiceProvider serviceProvider, IObjectLoader objectLoader);
    }
}
