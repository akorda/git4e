using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Git4e
{
    public class LazyHashableObjectList<T> : List<LazyHashableObject<T>>
        where T : IHashableObject
    {
        [JsonIgnore]
        public IHashableObject Parent { get; set; }

        public LazyHashableObjectList()
        {
        }

        public LazyHashableObjectList(IEnumerable<LazyHashableObject<T>> collection)
            : base(collection)
        {
        }

        public LazyHashableObjectList(int capacity)
            : base(capacity)
        {
        }

        public LazyHashableObject<T> RefreshItem(LazyHashableObject<T> item)
        {
            var removed = this.Remove(item);
            var newItem = new LazyHashableObject<T>(item.FinalValue);
            this.Add(newItem);
            this.Parent?.MarkAsDirty();
            return newItem;
        }
    }
}
