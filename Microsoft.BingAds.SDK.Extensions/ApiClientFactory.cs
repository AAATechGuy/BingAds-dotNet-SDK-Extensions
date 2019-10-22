using Microsoft.BingAds.V13.AdInsight;
using Microsoft.BingAds.V13.Bulk;
using Microsoft.BingAds.V13.CampaignManagement;
using Microsoft.BingAds.V13.CustomerBilling;
using Microsoft.BingAds.V13.CustomerManagement;
using Microsoft.BingAds.V13.Reporting;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.BingAds.SDK.Extensions
{
    /// <summary>
    /// Factory for BingAdsApi clients
    /// </summary>
    public sealed class ApiClientFactory
    {
        private AuthData authDataTemplate;
        private IAuthenticationTokenFactory authTokenFactory;
        private ApiType apiType;
        private ApiEnvironment apiEnvironment;

        public ApiClientFactory(
            string developerToken,
            IAuthenticationTokenFactory authTokenFactory,
            long? customerId = null,
            long? customerAccountId = null,
            ApiEnvironment apiEnvironment = ApiEnvironment.Production,
            ApiType apiType = ApiType.V13)
        {
            if (string.IsNullOrWhiteSpace(developerToken))
                throw new ArgumentNullException(nameof(developerToken));
            if (authTokenFactory == null)
                throw new ArgumentNullException(nameof(authTokenFactory));

            this.apiType = apiType;
            this.apiEnvironment = apiEnvironment;
            this.authTokenFactory = authTokenFactory;
            this.authDataTemplate = new AuthData()
            {
                DeveloperToken = developerToken,
                CustomerId = customerId?.ToString(),
                CustomerAccountId = customerAccountId?.ToString(),
                HeaderNamespace = Config.HeaderNamespace[apiEnvironment]
            };
        }

        /// <summary>
        /// Factory for ApiV13Client. 
        /// </summary>
        /// <param name="binding">optional Binding value</param>
        /// <returns>client</returns>
        public ApiV13Client CreateApiV13Client(Binding binding = null)
        {
            if (apiType != ApiType.V13)
                throw new InvalidOperationException("ApiType is not set correctly in factory");

            Logger.Verbose($"called");
            var apiClient = new ApiV13Client()
            {
                AdInsightApi = new Lazy<ApiClient<IAdInsightService>>(() =>
                    CreateApiClient<IAdInsightService>(
                        Config.EndpointUrlMap[apiEnvironment][apiType][nameof(IAdInsightService)], binding)),
                BulkApi = new Lazy<ApiClient<IBulkService>>(() =>
                    CreateApiClient<IBulkService>(
                        Config.EndpointUrlMap[apiEnvironment][apiType][nameof(IBulkService)], binding)),
                CampaignManagementApi = new Lazy<ApiClient<ICampaignManagementService>>(() =>
                    CreateApiClient<ICampaignManagementService>(
                        Config.EndpointUrlMap[apiEnvironment][apiType][nameof(ICampaignManagementService)], binding)),
                CustomerBillingApi = new Lazy<ApiClient<ICustomerBillingService>>(() =>
                    CreateApiClient<ICustomerBillingService>(
                        Config.EndpointUrlMap[apiEnvironment][apiType][nameof(ICustomerBillingService)], binding)),
                CustomerManagementApi = new Lazy<ApiClient<ICustomerManagementService>>(() =>
                    CreateApiClient<ICustomerManagementService>(
                        Config.EndpointUrlMap[apiEnvironment][apiType][nameof(ICustomerManagementService)], binding)),
                ReportingApi = new Lazy<ApiClient<IReportingService>>(() =>
                    CreateApiClient<IReportingService>(
                        Config.EndpointUrlMap[apiEnvironment][apiType][nameof(IReportingService)], binding))
            };

            return apiClient;
        }

        /// <summary>
        /// Factory for ApiClient<TClient>
        /// </summary>
        /// <typeparam name="TClient">BingAdsApi service type</typeparam>
        /// <param name="endpointUrl">optional service endpointUrl</param>
        /// <param name="binding">optional Binding value</param>
        /// <returns>client</returns>
        public ApiClient<TClient> CreateApiClient<TClient>(string endpointUrl = null, Binding binding = null)
            where TClient : class
        {
            Logger.Verbose($"called with TClient={typeof(TClient).Name}");

            endpointUrl = endpointUrl ?? Config.EndpointUrlMap[apiEnvironment][apiType][typeof(TClient).Name];
            binding = binding ?? new BasicHttpsBinding() { MaxReceivedMessageSize = int.MaxValue };

            var channelFactory = new ChannelFactory<TClient>(binding, new EndpointAddress(endpointUrl));

            // add endpointBehavior to embed AuthData in headers for all requests from client. 
            var authEndpointBehavior = new AuthEndpointBehavior(authDataTemplate, authTokenFactory);
            channelFactory.Endpoint.EndpointBehaviors.Add(authEndpointBehavior);

            var client = channelFactory.CreateChannel(); // create a client from channelFactory

            var apiClient = new ApiClient<TClient>() { Client = client };
            apiClient.ActionOnDispose = () =>
            {
                try { ((IDisposable)client)?.Dispose(); } catch { }
                try { channelFactory.Close(); } catch { }
            };

            return apiClient;
        }
    }
}