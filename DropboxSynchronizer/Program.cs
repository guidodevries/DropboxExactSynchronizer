using System;
using System.Configuration;
using System.IO;

using DropboxSynchronizer.Dropbox;
using DropboxSynchronizer.Exact;

using Spring.Social.Dropbox.Api;

namespace DropboxSynchronizer
{
    /// <summary>
    /// Main program starting initiating the Dropbox and Exact authentication and then start the synchronizer.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main program entrypoint.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            DropboxScanner dropboxScanner = null;

            try
            {
                AuthenticateExactOnline(
                    (success, documentService) =>
                    {
                        if (success)
                        {
                            var dropboxFolder = ConfigurationManager.AppSettings["DropboxFolder"];
                            AuthenticateDropbox(
                                (dropBoxAuthenticationCompleted, dropbox) =>
                                {
                                    var timer = new Timer();
                                    var localFileCache = new LocalFileCache();
                                    dropboxScanner = new DropboxScanner(
                                        timer,
                                        dropbox,
                                        documentService,
                                        dropboxFolder);

                                    dropboxScanner.Start();

                                    //var documents = documentService.GetDocuments();
                                    //var attachments = documentService.GetDocumentAttachments();

                                    //var content = File.ReadAllBytes(@"D:\Users\gdvries\Desktop\Test\Test.txt");
                                    //documentService.StoreDocument("NewDocumentFromCode.txt", content);

                                });
                        }
                    });

                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
            finally
            {
                if (dropboxScanner != null)
                {
                    dropboxScanner.Stop();
                }
            }
        }

        /// <summary>
        /// Force the user to first authenticate at Dropbox.
        /// </summary>
        /// <param name="callBack">The call back returning a value indicating success and the Dropbox API.</param>
        private static void AuthenticateDropbox(Action<bool, IDropbox> callBack)
        {
            var dropboxKey = ConfigurationManager.AppSettings["DropboxKey"];
            var dropboxSecret = ConfigurationManager.AppSettings["DropboxSecret"];
            var dropboxRedirectUri = ConfigurationManager.AppSettings["DropboxRedirectUri"];

            var dropboxBroker = new DropboxAuthorizationBroker(dropboxKey, dropboxSecret, dropboxRedirectUri);
            dropboxBroker.Authenticate(callBack);
        }

        /// <summary>
        /// Force the user to first authenticate at Exact.
        /// </summary>
        /// <param name="callBack">The call back returning a value indicating success and the authorizationBroker.</param>
        private static void AuthenticateExactOnline(Action<bool, ExactDocumentService> callBack)
        {
            var exactKey = ConfigurationManager.AppSettings["ExactKey"];
            var exactSecret = ConfigurationManager.AppSettings["ExactSecret"];
            var exactBaseUriString = ConfigurationManager.AppSettings["ExactBaseUri"];
            var exactCallbackUriString = ConfigurationManager.AppSettings["ExactRedirectUri"];

            var exactBaseUri = new Uri(exactBaseUriString);
            var exactCallbackUri = new Uri(exactCallbackUriString);

            var broker = new ExactAuthorizationBroker(exactBaseUri, exactKey, exactSecret, exactCallbackUri);
            broker.Authenticate(
                succes =>
                {
                    ExactDocumentService documentService = null;
                    if (succes)
                    {
                        documentService = new ExactDocumentService(exactBaseUri, broker);
                    }

                    callBack(succes, documentService);
                });
        }
    }
}
