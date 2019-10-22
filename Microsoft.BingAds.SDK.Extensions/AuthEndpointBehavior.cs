using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace Microsoft.BingAds.SDK.Extensions
{
    /// <summary>
    /// AuthEndpointBehavior helps embed AuthData in headers for all requests from client.
    /// </summary>
    internal sealed class AuthEndpointBehavior : IEndpointBehavior
    {
        private AuthData authDataTemplate;
        private IAuthenticationTokenFactory authTokenFactory;
        internal AuthEndpointBehavior(AuthData authDataTemplate, IAuthenticationTokenFactory authTokenFactory)
            => (this.authDataTemplate, this.authTokenFactory) = (authDataTemplate, authTokenFactory);

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            => clientRuntime.ClientMessageInspectors.Add(new AuthMessageInspector(authDataTemplate, authTokenFactory));

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
        public void Validate(ServiceEndpoint endpoint) { }

        #region AuthMessageInspector

        private sealed class AuthMessageInspector : IClientMessageInspector
        {
            private AuthData authDataTemplate;
            private IAuthenticationTokenFactory authTokenFactory;
            internal AuthMessageInspector(AuthData authDataTemplate, IAuthenticationTokenFactory authTokenFactory)
                => (this.authDataTemplate, this.authTokenFactory) = (authDataTemplate, authTokenFactory);

            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                if (authTokenFactory == null)
                    return null;

                var authData = authDataTemplate.Clone();
                using (var cts = new CancellationTokenSource(Config.DefaultAuthTokenFetchTimeout))
                {
                    // fetch AuthToken from AuthTokenFactory
                    var authToken = authTokenFactory.CreateAuthenticationTokenAsync(cts.Token).Result;
                    authData.AuthenticationToken = authToken;
                }

                // update header with authData, if requested and if exists
                TryUpdateHeader(request.Headers, nameof(AuthData.DeveloperToken), authData.DeveloperToken, authData.HeaderNamespace);
                TryUpdateHeader(request.Headers, nameof(AuthData.AuthenticationToken), authData.AuthenticationToken, authData.HeaderNamespace);
                TryUpdateHeader(request.Headers, nameof(AuthData.CustomerId), authData.CustomerId, authData.HeaderNamespace);
                TryUpdateHeader(request.Headers, nameof(AuthData.CustomerAccountId), authData.CustomerAccountId, authData.HeaderNamespace);

                var correlationState = Guid.NewGuid();
                Logger.Verbose($"correlationState={correlationState}");
                return correlationState;
            }

            private static void TryUpdateHeader(MessageHeaders headers, string headerName, object value, string ns2)
            {
                if (value == null)
                    return;

                int existingHeaderIndex = -1;
                if (headers.Any(h => { ++existingHeaderIndex; return h.Name == headerName; }))
                {
                    var ns = headers[existingHeaderIndex].Namespace;
                    headers.RemoveAt(existingHeaderIndex);
                    headers.Add(MessageHeader.CreateHeader(headerName, ns, value));
                    Logger.Verbose($"updated header {headerName}");
                }
            }

            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                Logger.Verbose($"correlationState={correlationState}");
            }
        }

        #endregion AuthMessageInspector
    }

    internal sealed class AuthData
    {
        public string AuthenticationToken { get; set; }
        public string CustomerAccountId { get; set; }
        public string CustomerId { get; set; }
        public string DeveloperToken { get; set; }
        public string HeaderNamespace { get; set; }

        internal AuthData Clone() => (AuthData)this.MemberwiseClone();
    }
}