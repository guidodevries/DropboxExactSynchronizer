using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

using DropboxSynchronizer.Exact;
using DropboxSynchronizer.Interfaces;

using Spring.Social.Dropbox.Api;
using Spring.Social.Dropbox.Connect;
using Spring.Social.OAuth1;

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
            try
            {
                AuthenticateExactOnline(
                    (success, documentService) =>
                    {
                        if (success)
                        {
                            var documents = documentService.GetDocuments();
                            var attachments = documentService.GetDocumentAttachments();

                            var content = File.ReadAllBytes(@"D:\Users\gdvries\Desktop\Test\Test.txt");

                            documentService.StoreDocument("NewDocumentFromCode.txt", content);
                        }
                    });


                //var dropboxFolder = ConfigurationManager.AppSettings["DropboxFolder"];
                //var dropbox = AuthenticateDropbox();

                //var timer = new Timer();
                //var localFileCache = new LocalFileCache();
                //var dropboxScanner = new DropboxScanner(timer, dropbox, localFileCache, dropboxFolder);

                //dropboxScanner.Start();

                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                    {
                        if (ex is DropboxApiException)
                        {
                            Console.WriteLine(ex.Message);
                            return true;
                        }
                        return false;
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Setup the authentication for dropbox.
        /// </summary>
        /// <returns>The dropbox API.</returns>
        private static IDropbox AuthenticateDropbox()
        {
            var dropboxKey = ConfigurationManager.AppSettings["DropboxKey"];
            var dropboxSecret = ConfigurationManager.AppSettings["DropboxSecret"];

            var dropboxServiceProvider = new DropboxServiceProvider(dropboxKey, dropboxSecret, AccessLevel.Full);
            var oauthToken = dropboxServiceProvider.OAuthOperations.FetchRequestTokenAsync(null, null).Result;

            var parameters = new OAuth1Parameters { CallbackUrl = @"http://localhost:8000/dropbox/callback" };
            var authenticateUrl = dropboxServiceProvider.OAuthOperations.BuildAuthorizeUrl(oauthToken.Value, parameters);

            string authenticationCode;

            var authenticationCodeService = RedirectUriService.Instance;
            authenticationCodeService.OnAuthenticatedCodeRecieved += (s, e) => authenticationCode = e;

            var serviceHost = new WebServiceHost(authenticationCodeService, new Uri("http://localhost:8000/"));
            serviceHost.AddServiceEndpoint(typeof(IRedirectUriService), new WebHttpBinding(), string.Empty);

            try
            {
                serviceHost.Open();

                Process.Start(authenticateUrl);
                Console.Write("Press any key when authorization attempt has succeeded");
                Console.ReadLine();

                var requestToken = new AuthorizedRequestToken(oauthToken, null);
                var oauthAccessToken = dropboxServiceProvider.OAuthOperations.ExchangeForAccessTokenAsync(requestToken, null).Result;

                return dropboxServiceProvider.GetApi(oauthAccessToken.Value, oauthAccessToken.Secret);
            }
            finally
            {
                serviceHost.Close();
            }
        }

        /// <summary>
        /// Force the user to first authenticate.
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
