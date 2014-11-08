namespace DropboxSynchronizer.Interfaces
{
    /// <summary>
    /// Interface of the document data entity.
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        string Subject { get; }
    }
}
