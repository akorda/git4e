using System.IO;

namespace Git4e
{
    public class PhysicalFilesObjectStoreOptions
    {
        public const string DefaultRootDirectoryName = ".git4e";
        public const string DefaultObjectDirectoryName = "objects";
        public const string DefaultRefsDirectoryName = "refs";
        public const string DefaultInitialBranchName = "main";

        public string RootDirectory { get; set; }
        public string ObjectsDirectory { get => Path.Combine(this.RootDirectory, DefaultObjectDirectoryName); }
        public string RefsDirectory { get => Path.Combine(this.RootDirectory, DefaultRefsDirectoryName); }
        public string InitialBranchName { get; set; } = DefaultInitialBranchName;
    }
}
