using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using DropboxSynchronizer.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DropboxSynchronizer.Exact
{
    /// <summary>
    /// Authorization broker with Exact Online specifics.
    /// </summary>
    public class ExactAuthorizationBroker : IAuthorizationBroker
    {
        #region Private Fields

        private const string AuthenticationUriString = @"api/oauth2/auth";
        private const string AccessTokenUriString = @"api/oauth2/token";

        private TimeSpan accessTokenExpirationDuration = new TimeSpan(0, 0, 600);

        private readonly string exactKey;
        private readonly string exactSecret;

        private readonly Uri exactBaseUri;
        private readonly Uri exactCallbackUri;

        private readonly RestClient restClient;

        private Process browserInstance;

        private DateTime accessRetrievalMoment = DateTime.MinValue;
        private string accessToken;
        private string refreshToken;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExactAuthorizationBroker"/> class.
        /// </summary>
        /// <param name="exactBaseUri">The exact base URI.</param>
        /// <param name="exactKey">The exact key.</param>
        /// <param name="exactSecret">The exact secret.</param>
        /// <param name="exactCallbackUri">The exact callback URI.</param>
        public ExactAuthorizationBroker(Uri exactBaseUri, string exactKey, string exactSecret, Uri exactCallbackUri)
        {
            this.exactBaseUri = exactBaseUri;
            this.exactKey = exactKey;
            this.exactSecret = exactSecret;
            this.exactCallbackUri = exactCallbackUri;

            this.restClient = new RestClient(exactBaseUri);
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
                return this.accessToken;
            }
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        public string RefreshToken
        {
            get
            {
                return this.refreshToken;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setup the authentication in order to retrieve an access token.
        /// </summary>
        /// <param name="callBack">The call back indicating whether the action was successful.</param>
        public void Authenticate(Action<bool> callBack)
        {
            if (this.accessToken == null)
            {
                this.RequestAuthenticationCode(
                    authenticationCode =>
                    {
                        if (string.IsNullOrEmpty(authenticationCode))
                        {
                            callBack(false);
                        }

                        this.RequestAccessToken(authenticationCode, callBack);
                    });
            }
            else
            {
                this.RefreshAccessTokenIfNeeded(callBack);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Requests the authentication code.
        /// </summary>
        /// <param name="callBack">The call back which is invoked with the retrieved authentication code.</param>
        private void RequestAuthenticationCode(Action<string> callBack)
        {
            var authUri = new Uri(this.exactBaseUri, AuthenticationUriString);

            var authenticationCodeService = RedirectUriService.Instance;
            authenticationCodeService.OnAuthenticatedCodeRecieved += (s, e) =>
            {
                if (this.browserInstance != null)
                {
                    this.browserInstance.CloseMainWindow();
                    this.browserInstance = null;
                }

                callBack(e);
            };

            var serviceHost = new WebServiceHost(authenticationCodeService, new Uri("http://localhost:8000/"));
            serviceHost.AddServiceEndpoint(typeof(IRedirectUriService), new WebHttpBinding(), string.Empty);

            try
            {
                serviceHost.Open();

                var authenticationRequestUriString = new StringBuilder(authUri.ToString());
                authenticationRequestUriString.AppendFormat("?client_id={0}&", this.exactKey);
                authenticationRequestUriString.AppendFormat("redirect_uri={0}&", @"http://localhost:8000/oauth2/callback");
                authenticationRequestUriString.AppendFormat("response_type={0}", "code");

                // Keep a reference in order to stop the process afterwards.
                this.browserInstance = Process.Start(authenticationRequestUriString.ToString());

                if (this.browserInstance != null)
                {
                    this.browserInstance.WaitForExit();
                }
            }
            catch
            {
                callBack(string.Empty);
            }
            finally
            {
                serviceHost.Close();
            }
        }

        /// <summary>
        /// Requests the access token.
        /// </summary>
        /// <param name="authenticationCode">The authentication code.</param>
        /// <param name="callBack">The call back containing a value that indicates whether the access token was retrieved.</param>
        private void RequestAccessToken(string authenticationCode, Action<bool> callBack)
        {
            try
            {
                var accessTokenRequest = new RestRequest(AccessTokenUriString, Method.POST);
                accessTokenRequest.RequestFormat = DataFormat.Json;

                accessTokenRequest.AddParameter("code", authenticationCode);
                accessTokenRequest.AddParameter("redirect_uri", this.exactCallbackUri.ToString());
                accessTokenRequest.AddParameter("grant_type", "authorization_code");
                accessTokenRequest.AddParameter("client_id", this.exactKey);
                accessTokenRequest.AddParameter("client_secret", this.exactSecret);
                accessTokenRequest.AddParameter("force_login", 0);

                var response = this.restClient.Execute(accessTokenRequest);
                var accessTokenData = JObject.Parse(response.Content);

                this.accessToken = accessTokenData["access_token"].ToString();
                this.refreshToken = accessTokenData["refresh_token"].ToString();
                this.accessTokenExpirationDuration = new TimeSpan(0, 0, (int)accessTokenData["expires_in"]);
                this.accessRetrievalMoment = DateTime.Now;

                callBack(true);
            }
            catch
            {
                callBack(false);
            }
        }

        /// <summary>
        /// Refreshes the access token if needed.
        /// </summary>
        /// <param name="callBack">The call back containing a value that indicates whether the access token was retrieved..</param>
        private void RefreshAccessTokenIfNeeded(Action<bool> callBack)
        {
            if (DateTime.Now.Subtract(this.accessRetrievalMoment) >= this.accessTokenExpirationDuration)
            {
                var accessTokenRequest = new RestRequest(AccessTokenUriString, Method.POST);
                accessTokenRequest.RequestFormat = DataFormat.Json;

                accessTokenRequest.AddParameter("refresh_token", this.refreshToken);
                accessTokenRequest.AddParameter("grant_type", "refresh_token");
                accessTokenRequest.AddParameter("client_id", this.exactKey);
                accessTokenRequest.AddParameter("client_secret", this.exactSecret);

                var response = this.restClient.Execute(accessTokenRequest);
                var accessTokenData = JObject.Parse(response.Content);

                this.accessToken = accessTokenData["access_token"].ToString();
                this.refreshToken = accessTokenData["refresh_token"].ToString();
                this.accessTokenExpirationDuration = new TimeSpan(0, 0, (int)accessTokenData["expires_in"]);
                this.accessRetrievalMoment = DateTime.Now;
            }

            callBack(true);
        }

        #endregion
    }
}
