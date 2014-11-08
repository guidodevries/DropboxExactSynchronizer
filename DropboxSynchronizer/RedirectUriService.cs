using System;
using System.ServiceModel;

using DropboxSynchronizer.Interfaces;

namespace DropboxSynchronizer
{
    /// <summary>
    /// Singleton service that handles the RedirectUri invocation during the authorization steps.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RedirectUriService : IRedirectUriService
    {
        #region Private Fields

        private readonly static RedirectUriService instance = new RedirectUriService();

        #endregion

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="RedirectUriService"/> class from being created.
        /// </summary>
        private RedirectUriService()
        {
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when the on authenticated code was retrieved.
        /// </summary>
        public event EventHandler<string> OnAuthenticatedCodeRecieved;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a singleton instance of the service.
        /// </summary>
        public static RedirectUriService Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Handler of the RedirectUri invocation of Exact Online.
        /// </summary>
        /// <param name="code">The code.</param>
        public void InvokeRedirectUri(string code)
        {
            this.RaiseAuthenticationCompleted(code);
        }

        /// <summary>
        /// Handler of the RedirectUri invocation of DropBox.
        /// </summary>
        /// <param name="oauth_token">The oauth_token.</param>
        /// <param name="uid">The user id.</param>
        public void InvokeRedirectUri(string oauth_token, string uid)
        {
            this.RaiseAuthenticationCompleted(oauth_token);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Raises the authentication completed event.
        /// </summary>
        /// <param name="code">The code.</param>
        private void RaiseAuthenticationCompleted(string code)
        {
            var handler = this.OnAuthenticatedCodeRecieved;

            if (handler != null)
            {
                handler(this, code);
            }
        }

        #endregion
    }
}
