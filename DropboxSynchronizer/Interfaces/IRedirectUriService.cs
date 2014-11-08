using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace DropboxSynchronizer.Interfaces
{
    /// <summary>
    /// Interface of the RedirectUriService.
    /// </summary>
    [ServiceContract]
    public interface IRedirectUriService
    {
        /// <summary>
        /// Occurs when the on authenticated code was retrieved.
        /// </summary>
        event EventHandler<string> OnAuthenticatedCodeRecieved;

        /// <summary>
        /// Handler of the RedirectUri invocation.
        /// </summary>
        /// <param name="code">The code.</param>
        [OperationContract(Name = @"oauth2/callback")]
        [WebGet]
        void InvokeRedirectUri(string code);

        /// <summary>
        /// Handler of the RedirectUri invocation.
        /// </summary>
        /// <param name="oauth_token">The oauth_token.</param>
        /// <param name="uid">The user id.</param>
        [OperationContract(Name = @"dropbox/callback")]
        [WebGet]
        void InvokeRedirectUri(string oauth_token, string uid);
    }
}
