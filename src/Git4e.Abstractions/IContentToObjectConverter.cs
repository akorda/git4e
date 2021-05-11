using System;

namespace Git4e
{
    public interface IContentToObjectConverter
    {
        IHashableObject ToObject(IRepository repository, object content);
    }
}
