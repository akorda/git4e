namespace Git4e
{
    /// <summary>
    /// Error codes used in <c>git4e</c> library.
    /// </summary>
    public enum Git4eErrorCode
    {
        /// <summary>
        /// Not an error.
        /// </summary>
        Ok,
        /// <summary>
        /// General error.
        /// </summary>
        GenericError,
        /// <summary>
        /// An item (e.g. a commit, a branch, etc.) was not found.
        /// </summary>
        NotFound,
        /// <summary>
        /// An item (e.g. a branch) already exists.
        /// </summary>
        Exists,
        /// <summary>
        /// A stored object has not the expected content type
        /// </summary>
        InvalidContentType,
        /// <summary>
        /// A reference to a commit parent is not valid
        /// </summary>
        InvalidCommitParent
    }
}
