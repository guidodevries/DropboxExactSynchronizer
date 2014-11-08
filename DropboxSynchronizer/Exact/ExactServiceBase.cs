using System;
using System.Threading.Tasks;

using DropboxSynchronizer.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DropboxSynchronizer.Exact
{
    public class ExactServiceBase
    {
        #region Private Fields

        private IAuthorizationBroker authorizationBroker;
        private RestClient restClient;

        private string currentDivision;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExactServiceBase" /> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="authorizationBroker">The authorization broker.</param>
        public ExactServiceBase(Uri baseUri, IAuthorizationBroker authorizationBroker)
        {
            this.restClient = new RestClient(baseUri);
            this.restClient.AddHandler("application/octet-stream", new RestSharp.Deserializers.JsonDeserializer());

            this.authorizationBroker = authorizationBroker;
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Creates a new request based on the provided resource en method.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="method">The method.</param>
        /// <returns>A new request.</returns>
        protected IRestRequest CreateRequest(string resource, Method method)
        {
            return new RestRequest(resource, method);
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <typeparam name="T">Type to parse the result to.</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>A parsed result of the specified type.</returns>
        protected T ExecuteRequest<T>(IRestRequest request)
        {
            var response = this.ExecuteRequest(request);
            return JObject.Parse(response.Content).SelectToken("d.results").ToObject<T>();
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response of the request.</returns>
        protected IRestResponse ExecuteRequest(IRestRequest request)
        {
            Task task = Task.Factory.StartNew(() => this.authorizationBroker.Authenticate(succes => { }));
            task.Wait();

            this.AddHeaders(request);
            request.AddUrlSegment("division", this.GetCurrentDivision());

            var response = this.restClient.Execute(request);

            this.RaiseExceptionInCaseOfErrors(response);

            return response;
        }

        /// <summary>
        /// Raises the exception in case of errors in the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <exception cref="System.InvalidOperationException">
        /// No content was returned
        /// or
        /// An error was returned
        /// </exception>
        private void RaiseExceptionInCaseOfErrors(IRestResponse response)
        {
            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("No content was returned");
            }

            var responseObject = JObject.Parse(response.Content);
            var error = responseObject["error"];

            if (error != null)
            {
                var errorMessage = error["message"]["value"];
                throw new InvalidOperationException("An error was returned", new Exception(errorMessage.ToString()));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the authentication and JSON headers to the provided request.
        /// </summary>
        /// <param name="request">The request.</param>
        private void AddHeaders(IRestRequest request)
        {
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", "Bearer " + this.authorizationBroker.AccessToken);
        }

        /// <summary>
        /// Gets the current division.
        /// </summary>
        /// <returns>The current division</returns>
        private string GetCurrentDivision()
        {
            if (string.IsNullOrEmpty(this.currentDivision))
            {
                Task task = Task.Factory.StartNew(() => this.authorizationBroker.Authenticate(succes => { }));
                task.Wait();

                var request = this.CreateRequest(@"api/v1/current/Me", Method.GET);

                this.AddHeaders(request);

                var response = this.restClient.Execute(request);

                this.currentDivision = JObject.Parse(response.Content).SelectToken("d.results[0].CurrentDivision").ToObject<string>();
            }

            return this.currentDivision;
        }

        #endregion
    }
}
