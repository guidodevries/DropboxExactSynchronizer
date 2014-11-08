namespace DropboxSynchronizer.Interfaces
{
    /// <summary>
    /// Interface of the local file cache.
    /// </summary>
    public interface IFileStore
    {
        /// <summary>
        /// Stores the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="content">The content.</param>
        void StoreFile(string fileName, byte[] content);
    }
}
