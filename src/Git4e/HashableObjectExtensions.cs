using System.Collections.Generic;
using System.Linq;

namespace Git4e
{
    public static class HashableObjectExtensions
    {
        public static IEnumerable<IHashableObject> GetAllChildObjects(this IHashableObject root)
        {
            var childObjects = new List<IHashableObject>();
            childObjects.AddRange(root.ChildObjects);
            foreach (var child in root.ChildObjects)
            {
                childObjects.AddRange(child.GetAllChildObjects());
            }
            foreach (var child in childObjects.Distinct())
            {
                yield return child;
            }
        }
    }
}
