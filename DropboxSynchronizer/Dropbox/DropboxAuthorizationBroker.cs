using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Web;

using DropboxSynchronizer.Interfaces;

using Spring.Social.Dropbox.Api;
using Spring.Social.Dropbox.Connect;
using Spring.Social.OAuth1;

namespace DropboxSynchronizer.Dropbox
{
    /// <summary>
    /// Authorization broker for Dropbox
    /// </summary>
    public class DropboxAuthorizationBroker
    {
        #region Private Fields

        private readonly string dropboxKey;
        private readonly string dropboxSecret;
        private readonly string dropboxRedirectUri;

        private Process browserInstance;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxAuthorizationBroker" /> class.
        /// </summary>
        /// <param name="dropboxKey">The dropbox key.</param>
        /// <param name="dropboxSecret">The dropbox secret.</param>
        /// <param name="dropboxRedirectUri">The dropbox redirect URI.</param>
        public DropboxAuthorizationBroker(string dropboxKey, string dropboxSecret, string dropboxRedirectUri)
        {
            this.dropboxKey = dropboxKey;
            this.dropboxSecret = dropboxSecret;
            this.dropboxRedirectUri = dropboxRedirectUri;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the access token.
        /// </summary>
        public string AccessToken
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        public string RefreshToken
        {
            get
            {
                return string.Empty;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setup the authentication in order to retrieve an access token.
        /// </summary>
        /// <param name="callBack">The call back indicating whether the action was successful.</param>
        public void Authenticate(Action<bool, IDropbox> callBack)
        {
            var dropboxServiceProvider = new DropboxServiceProvider(this.dropboxKey, this.dropboxSecret, AccessLevel.Full);
            var oauthToken = dropboxServiceProvider.OAuthOperations.FetchRequestTokenAsync(null, null).Result;

            var parameters = new OAuth1Parameters { CallbackUrl = this.dropboxRedirectUri };
            var authenticateUrl = dropboxServiceProvider.OAuthOperations.BuildAuthorizeUrl(oauthToken.Value, parameters);

            var authenticationCodeService = RedirectUriService.Instance;
            authenticationCodeService.OnAuthenticatedCodeRecieved +=
                (s, e) =>
                {
                    if (this.browserInstance != null)
                    {
                        this.browserInstance.CloseMainWindow();
                        this.browserInstance = null;
                    }

                    var requestToken = new AuthorizedRequestToken(oauthToken, null);
                    var oauthAccessToken = dropboxServiceProvider.OAuthOperations.ExchangeForAccessTokenAsync(requestToken, null).Result;

                    callBack(true, dropboxServiceProvider.GetApi(oauthAccessToken.Value, oauthAccessToken.Secret));
                };

            var serviceHost = new WebServiceHost(authenticationCodeService, new Uri("http://localhost:8000/"));
            serviceHost.AddServiceEndpoint(typeof(IRedirectUriService), new WebHttpBinding(), string.Empty);

            try
            {
                serviceHost.Open();

                this.browserInstance = Process.Start(authenticateUrl);

                if (this.browserInstance != null)
                {
                    this.browserInstance.WaitForExit();
                }
            }
            finally
            {
                serviceHost.Close();
            }
        }

        #endregion
    }
}
