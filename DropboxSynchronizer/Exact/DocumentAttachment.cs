using DropboxSynchronizer.Interfaces;

using Newtonsoft.Json;

namespace DropboxSynchronizer.Exact
{
    /// <summary>
    /// Exact document attachment representation.
    /// </summary>
    internal class DocumentAttachment : IDocumentAttachment
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonProperty("ID")]
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the file content.
        /// </summary>
        [JsonProperty("Attachment")]
        public object Attachment { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [JsonProperty("FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        [JsonProperty("FileSize")]
        public int FileSize { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        [JsonProperty("Url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the document ID.
        /// </summary>
        [JsonProperty("Document")]
        public string Document { get; set; }
    }
}