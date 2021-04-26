using System;
using System.IO;

namespace Git4e
{
    public class PhysicalFilesObjectStoreOptions
    {
        public const string ObjectDirectoryName = "objects";

        public string RootDirectory { get; set; }

        public PhysicalFilesObjectStoreOptions()
        {
            this.RootDirectory = Path.Combine(Environment.CurrentDirectory, ObjectDirectoryName);
        }
    }
}
