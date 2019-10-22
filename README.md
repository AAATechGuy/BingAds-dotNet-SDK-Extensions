# BingAds-dotNet-SDK-Extensions

This extension library features:
1. MSAL.NET integration 
   - interactive UI to fetch AAD or WindowsLive credentials
   - auto-refresh of AccessToken on expiry
   - includes persistent cache of AccessTokens
2. V13ApiClient wrapper for clients of all six BingAdsApi services

## initializing client

```csharp
    
    // AuthenticationTokenFactory uses MSAL.NET to fetch tokens.
    // <param name="applicationClientId">ClientId for AAD application that accesses BingAdsApi</param>
    // <param name="usernameLoginHint">Username LoginHint to retrieve matching AccessToken from TokenCache</param>
    // <param name="tokenCacheCorrelationId">
    //   Used to identify persisted TokenCache location. 
    //   Provide a consistent non-null value across sessions for access to the same persisted cache. 
    //   If app must not need TokenCache persistence, just pass in null. 
    // </param>
    // <param name="accessTokenExpirationOffsetInSec">
    //   Specifies when a new Token will be refreshed silently, based on AccessToken's expiresOn property. 
    //   If AccessToken.ExpiresOn = "09:30" and offset = "-600", 
    //     then call to fetch accessToken after "09:20" will try to get a refreshed accessToken.
    // </param>
    var authDataFactory = new AuthenticationTokenFactory(
        applicationClientId,
        usernameLoginHint,
        tokenCacheCorrelationId: "default");
    
    // clientFactory creates service clients
    var clientFactory = new ApiClientFactory(developerToken, authDataFactory, customerId: null, accountId: null);
    
    // init v13ApiClient (contains all 6 BingAdsApi service clients)
    using (var v13ApiClient = clientFactory.CreateV13ApiClient())
    {
        //// your logic goes here
    }
    
    // init separate clients for each individual BingAdsApi service client
    using (var customerManagementApi = clientFactory.CreateApiClient<ICustomerManagementService>())
    {
        var client = customerManagementApi.Client;
        //// your logic goes here
    }
```


## V13ApiClient usage

```csharp
    using (var client = clientFactory.CreateV13ApiClient())
    {
        //// CustomerManagement
    
        Trace.WriteLine(Environment.NewLine + "> GetUser: ");
        var user = client.CustomerManagement.GetUser(new GetUserRequest() { });
    
        Trace.WriteLine(Environment.NewLine + "> FindAccounts: ");
        var accounts = client.CustomerManagement.FindAccounts(new FindAccountsRequest() { TopN = 10 });
    
        Trace.WriteLine(Environment.NewLine + "> GetAccount: ");
        var account = client.CustomerManagement.GetAccount(new GetAccountRequest() { AccountId = accountId });
    
        //// CustomerBilling
    
        Trace.WriteLine(Environment.NewLine + "> GetAccountMonthlySpend: ");
        var accountMonthlySpend = client.CustomerBilling.GetAccountMonthlySpend(
            new GetAccountMonthlySpendRequest()
            {
                AccountId = accountId,
                MonthYear = DateTime.UtcNow
            });
    
        //// CampaignManagement
    
        Trace.WriteLine(Environment.NewLine + "> GetCampaignsByAccountId: ");
        var campaignsByAccountId = client.CampaignManagement.GetCampaignsByAccountId(
            new GetCampaignsByAccountIdRequest()
            {
                //CustomerAccountId = accountId.ToString(), // set in header
                AccountId = accountId,
                CampaignType = CampaignType.Search,
            });
    
        //// Bulk
    
        Trace.WriteLine(Environment.NewLine + "> DownloadCampaignsByAccountIds: ");
        var downloadCampaignsByAccountIdResponse = client.Bulk.DownloadCampaignsByAccountIds(
            new DownloadCampaignsByAccountIdsRequest()
            {
                //CustomerAccountId = accountId.ToString(), // set in header
                AccountIds = new long[] { accountId },
                DataScope = DataScope.EntityData,
                DownloadEntities = new DownloadEntity[] { DownloadEntity.Campaigns },
                FormatVersion = "6.0"
            });
    
        //// Reporting
    
        Trace.WriteLine(Environment.NewLine + "> SubmitGenerateReport: ");
        var reportResponse = client.Reporting.SubmitGenerateReport(
            new SubmitGenerateReportRequest()
            {
                ReportRequest = new BudgetSummaryReportRequest
                {
                    ReportName = Guid.NewGuid().ToString(),
                    Format = ReportFormat.Tsv,
                    Scope = new AccountThroughCampaignReportScope
                    {
                        AccountIds = new long[] { accountId },
                    },
                    Time = new ReportTime
                    {
                        PredefinedTime = ReportTimePeriod.Last30Days
                    },
                    Columns = new[]
                    {
                        BudgetSummaryReportColumn.AccountId,
                        BudgetSummaryReportColumn.AccountName,
                        BudgetSummaryReportColumn.Date,
                        BudgetSummaryReportColumn.MonthlyBudget,
                        BudgetSummaryReportColumn.DailySpend
                    }
                }
            });
    
        if (reportResponse != null)
        {
            Trace.WriteLine(Environment.NewLine + "> PollGenerateReport: ");
            var pollGenerateReport = client.Reporting.PollGenerateReport(
                new PollGenerateReportRequest()
                {
                    ReportRequestId = reportResponse.ReportRequestId
                });
        }
    }
```

## individual ICustomerManagementService client usage

```csharp
    using (var customerManagementApi = clientFactory.CreateApiClient<ICustomerManagementService>())
    {
        var client = customerManagementApi.Client;
    
        Trace.WriteLine(Environment.NewLine + "> GetUser: ");
        var user = client.GetUser(new GetUserRequest() { });
    
        Trace.WriteLine(Environment.NewLine + "> FindAccounts: ");
        var accounts = client.FindAccounts(new FindAccountsRequest() { TopN = 10 });
    
        Trace.WriteLine(Environment.NewLine + "> GetAccount: ");
        var account = client.GetAccount(new GetAccountRequest() { AccountId = accountId });
    }
```


