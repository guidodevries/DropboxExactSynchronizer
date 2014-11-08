using System;
using System.Collections.Generic;
using DropboxSynchronizer.Interfaces;
using RestSharp;

namespace DropboxSynchronizer.Exact
{
    /// <summary>
    /// Service proxy specific for handling documents in Exact Online.
    /// </summary>
    public class ExactDocumentService : ExactServiceBase
    {
        #region Private Fields

        private const string DocumentUri = @"/api/v1/{division}/documents/Documents";
        private const string DocumentAttachmentUri = @"/api/v1/{division}/documents/DocumentAttachments";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExactDocumentService"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="broker">The broker.</param>
        public ExactDocumentService(Uri baseUri, IAuthorizationBroker broker)
            : base(baseUri, broker)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <returns>A collection of documents.</returns>
        public IEnumerable<IDocument> GetDocuments()
        {
            var request = this.CreateRequest(DocumentUri, Method.GET);
            return this.ExecuteRequest<List<Document>>(request);
        }

        /// <summary>
        /// Gets the document attachments.
        /// </summary>
        /// <returns>A collection of document attachments.</returns>
        public IEnumerable<IDocumentAttachment> GetDocumentAttachments()
        {
            var request = this.CreateRequest(DocumentAttachmentUri, Method.GET);
            return this.ExecuteRequest<List<DocumentAttachment>>(request);
        }

        /// <summary>
        /// Stores the document.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fileContent">Content of the file.</param>
        public void StoreDocument(string name, byte[] fileContent)
        {
            var newDocument = new Document
            {
                ID = Guid.NewGuid().ToString(),
                Subject = name,
                HasEmptyBody = true,
                Type = 183
            };

            var request = this.CreateRequest(DocumentUri, Method.POST);
            request.AddJsonBody(newDocument);
            this.ExecuteRequest(request);

            this.StoreAttachment(name, fileContent, newDocument.ID);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Stores the attachment.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="documentId">The document identifier.</param>
        private void StoreAttachment(string fileName, byte[] fileContent, string documentId)
        {
            var newAttachment = new DocumentAttachment
                                    {
                                        ID = Guid.NewGuid().ToString(),
                                        Attachment = Convert.ToBase64String(fileContent),
                                        FileName = fileName,
                                        FileSize = fileContent.Length,
                                        Document = documentId,
                                    };

            var request = this.CreateRequest(DocumentAttachmentUri, Method.POST);

            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddJsonBody(newAttachment);

            this.ExecuteRequest(request);
        }

        #endregion
    }
}
