using System;
using System.IO;

namespace Git4e
{
    public class PhysicalFilesObjectStoreOptions
    {
        public const string DefaultRootDirectoryName = ".git4e";
        public const string DefaultObjectDirectoryName = "objects";

        public string RootDirectory { get; set; }
        public string ObjectsDirectory { get => Path.Combine(this.RootDirectory, DefaultObjectDirectoryName); }
    }
}
