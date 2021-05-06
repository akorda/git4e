using System;

namespace Git4e
{
    public class Globals
    {
        public static IServiceProvider ServiceProvider { get; set; }
        public static IContentSerializer ContentSerializer { get; set; }
        public static IHashCalculator HashCalculator { get; set; }
        public static IObjectStore ObjectStore { get; set; }
        public static IObjectLoader ObjectLoader { get; set; }
        public static IContentTypeResolver ContentTypeResolver { get; set; }
        public static Func<string, string, LazyHashableObjectBase> RootFromHashCreator { get; set; }
        public static Func<IHashableObject, LazyHashableObjectBase> RootFromInstanceCreator { get; set; }
    }
}
