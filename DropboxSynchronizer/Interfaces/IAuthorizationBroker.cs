using System;

namespace DropboxSynchronizer.Interfaces
{
    /// <summary>
    /// Interface of a authorization broker responsible for retrieving an access token.
    /// </summary>
    public interface IAuthorizationBroker
    {
        /// <summary>
        /// Gets the access token.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        string RefreshToken { get; }

        /// <summary>
        /// Setup the authentication in order to retrieve an access token.
        /// </summary>
        /// <param name="callBack">The call back indicating whether the action was successful.</param>
        void Authenticate(Action<bool> callBack);
    }
}
