namespace DropboxSynchronizer.Interfaces
{
    /// <summary>
    /// Interface of a document attachment.
    /// </summary>
    public interface IDocumentAttachment
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        int FileSize { get; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Gets the document ID.
        /// </summary>
        string Document { get; }
    }
}
