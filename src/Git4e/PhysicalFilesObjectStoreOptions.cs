using System.IO;

namespace Git4e
{
    /// <summary>
    /// The options to initialize a <see cref="Git4e.PhysicalFilesObjectStore"/> instance.
    /// </summary>
    public class PhysicalFilesObjectStoreOptions
    {
        /// <summary>
        /// The default root directory name of a git4e repository.
        /// </summary>
        public const string DefaultRootDirectoryName = ".git4e";

        /// <summary>
        /// The default objects directory name of a git4e repository.
        /// </summary>
        public const string DefaultObjectDirectoryName = "objects";

        /// <summary>
        /// The default refs directory name of a git4e repository.
        /// </summary>
        public const string DefaultRefsDirectoryName = "refs";

        /// <summary>
        /// The default initial branch name of a git4e repository.
        /// </summary>
        public const string DefaultInitialBranchName = "main";

        /// <summary>
        /// The root directory of this git4e repository.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// The objects directory of this git4e repository.
        /// </summary>
        public string ObjectsDirectory { get => Path.Combine(this.RootDirectory, DefaultObjectDirectoryName); }

        /// <summary>
        /// The refs directory of this git4e repository.
        /// </summary>
        public string RefsDirectory { get => Path.Combine(this.RootDirectory, DefaultRefsDirectoryName); }

        /// <summary>
        /// The initial branch name of this git4e repository.
        /// </summary>
        public string InitialBranchName { get; set; } = DefaultInitialBranchName;
    }
}
