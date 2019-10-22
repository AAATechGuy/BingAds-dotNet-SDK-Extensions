using Microsoft.BingAds.V13.AdInsight;
using Microsoft.BingAds.V13.Bulk;
using Microsoft.BingAds.V13.CampaignManagement;
using Microsoft.BingAds.V13.CustomerBilling;
using Microsoft.BingAds.V13.CustomerManagement;
using Microsoft.BingAds.V13.Reporting;
using System;

namespace Microsoft.BingAds.SDK.Extensions
{
    /// <summary>
    /// Client wrapper for BingAdsApi
    /// </summary>
    /// <typeparam name="TClient">BingAdsApi service type</typeparam>
    public sealed class ApiClient<TClient> : IDisposable
        where TClient : class
    {
        public TClient Client { get; internal set; }

        internal Action ActionOnDispose { get; set; }

        public void Dispose()
        {
            try { ActionOnDispose?.Invoke(); } catch (Exception ex) { Logger.Error(ex.ToString()); }
        }
    }

    /// <summary>
    /// Client wrapper for all BingAdsApi service types. 
    /// </summary>
    public sealed class V13ApiClient : IDisposable
    {
        /// <summary>
        /// BingAdsApi AdInsight service client
        /// </summary>
        public IAdInsightService AdInsight => AdInsightApi.Value.Client;

        /// <summary>
        /// BingAdsApi Bulk service client
        /// </summary>
        public IBulkService Bulk => BulkApi.Value.Client;

        /// <summary>
        /// BingAdsApi CampaignManagement service client
        /// </summary>
        public ICampaignManagementService CampaignManagement => CampaignManagementApi.Value.Client;

        /// <summary>
        /// BingAdsApi CustomerBilling service client
        /// </summary>
        public ICustomerBillingService CustomerBilling => CustomerBillingApi.Value.Client;

        /// <summary>
        /// BingAdsApi CustomerManagement service client
        /// </summary>
        public ICustomerManagementService CustomerManagement => CustomerManagementApi.Value.Client;

        /// <summary>
        /// BingAdsApi Reporting service client
        /// </summary>
        public IReportingService Reporting => ReportingApi.Value.Client;

        internal Lazy<ApiClient<IAdInsightService>> AdInsightApi { get; set; }
        internal Lazy<ApiClient<IBulkService>> BulkApi { get; set; }
        internal Lazy<ApiClient<ICampaignManagementService>> CampaignManagementApi { get; set; }
        internal Lazy<ApiClient<ICustomerBillingService>> CustomerBillingApi { get; set; }
        internal Lazy<ApiClient<ICustomerManagementService>> CustomerManagementApi { get; set; }
        internal Lazy<ApiClient<IReportingService>> ReportingApi { get; set; }

        public void Dispose()
        {
            if (AdInsightApi.IsValueCreated) AdInsightApi.Value.Dispose();
            if (BulkApi.IsValueCreated) BulkApi.Value.Dispose();
            if (CampaignManagementApi.IsValueCreated) CampaignManagementApi.Value.Dispose();
            if (CustomerBillingApi.IsValueCreated) CustomerBillingApi.Value.Dispose();
            if (CustomerManagementApi.IsValueCreated) CustomerManagementApi.Value.Dispose();
            if (ReportingApi.IsValueCreated) ReportingApi.Value.Dispose();
        }
    }
}