using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.BingAds.SDK.Extensions
{
    /// <summary>
    /// Configuration
    /// </summary>
    public static class Config
    {
        internal const int DefaultAccessTokenExpirationOffsetInSec = -60;

        /// <summary>
        /// timeout for fetching new OAuth AccessToken
        /// </summary>
        public static TimeSpan DefaultAuthTokenFetchTimeout { get; set; } = TimeSpan.FromSeconds(300);

        /// <summary>
        /// Location (local directory path) to store persisted TokenCache
        /// </summary>
        public static string DefaultTokenCacheFileLocation { get; set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// RedirectUri used to fetch OAuth AccessToken
        /// </summary>
        public static Dictionary<ApiEnvironment, string> RedirectUri { get; private set; } = new Dictionary<ApiEnvironment, string>()
        {
            [ApiEnvironment.Production] = "https://login.microsoftonline.com/common/oauth2/nativeclient"
        };

        /// <summary>
        /// Scopes used to fetch OAuth AccessToken
        /// </summary>
        public static Dictionary<ApiEnvironment, string[]> Scopes { get; private set; } = new Dictionary<ApiEnvironment, string[]>()
        {
            [ApiEnvironment.Production] = new[] { "https://ads.microsoft.com/ads.manage offline_access" }
        };

        /// <summary>
        /// HeaderNamespace in message headers used to fetch OAuth AccessToken
        /// </summary>
        public static Dictionary<ApiEnvironment, string> HeaderNamespace { get; private set; } = new Dictionary<ApiEnvironment, string>()
        {
            [ApiEnvironment.Production] = "https://bingads.microsoft.com/Customer/v13"
        };

        /// <summary>
        /// EndpointUrl used to create BingAdsApi clients
        /// </summary>
        public static Dictionary<ApiEnvironment, Dictionary<ApiType, Dictionary<string, string>>> EndpointUrlMap { get; private set; } = new Dictionary<ApiEnvironment, Dictionary<ApiType, Dictionary<string, string>>>()
        {
            [ApiEnvironment.Production] = new Dictionary<ApiType, Dictionary<string, string>>()
            {
                [ApiType.V13] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(V13.AdInsight.IAdInsightService)] = "https://adinsight.api.bingads.microsoft.com/Api/Advertiser/AdInsight/v13/AdInsightService.svc",
                    [nameof(V13.Bulk.IBulkService)] = "https://bulk.api.bingads.microsoft.com/Api/Advertiser/CampaignManagement/V13/BulkService.svc",
                    [nameof(V13.CampaignManagement.ICampaignManagementService)] = "https://campaign.api.bingads.microsoft.com/Api/Advertiser/CampaignManagement/V13/CampaignManagementService.svc",
                    [nameof(V13.CustomerBilling.ICustomerBillingService)] = "https://clientcenter.api.bingads.microsoft.com/Api/Billing/v13/CustomerBillingService.svc",
                    [nameof(V13.CustomerManagement.ICustomerManagementService)] = "https://clientcenter.api.bingads.microsoft.com/Api/CustomerManagement/v13/CustomerManagementService.svc",
                    [nameof(V13.Reporting.IReportingService)] = "https://reporting.api.bingads.microsoft.com/Api/Advertiser/Reporting/V13/ReportingService.svc"
                }
            }
        };
    }
}