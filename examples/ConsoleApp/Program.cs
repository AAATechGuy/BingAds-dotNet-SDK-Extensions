using Microsoft.BingAds.SDK.Extensions;
using Microsoft.BingAds.V13.Bulk;
using Microsoft.BingAds.V13.CampaignManagement;
using Microsoft.BingAds.V13.CustomerBilling;
using Microsoft.BingAds.V13.CustomerManagement;
using Microsoft.BingAds.V13.Reporting;
using System;
using System.Diagnostics;

namespace ConsoleApp
{
    class Program
    {
        static Program()
        {
            // to ignore all version mismatch related errors
            AssemblyResolverUtility.SetDefaultAssemblyResolver();
        }

        static void Main(string[] args)
        {
            string developerToken = ""; // e.g., 229HQA18J1837151
            string applicationClientId = ""; // e.g., 3b2aace2-e7b1-4895-9a12-901bd5def710
            string usernameLoginHint = ""; // e.g., email@contoso.com
            long customerId = 1; // valid customerId
            long accountId = 2; // valid accountId

            Test_ApiClient(developerToken, applicationClientId, usernameLoginHint, customerId, accountId);

            Console.WriteLine(Environment.NewLine + "---done---");
            Console.ReadKey();
        }

        private static void Test_ApiClient(
            string developerToken,
            string applicationClientId,
            string usernameLoginHint,
            long customerId,
            long accountId)
        {
            var authDataFactory = new AuthenticationTokenFactory(
                applicationClientId,
                usernameLoginHint,
                tokenCacheCorrelationId: "default");

            var clientFactory = new ApiClientFactory(developerToken, authDataFactory, null, accountId);

            using (var customerManagementApi = clientFactory.CreateApiClient<ICustomerManagementService>())
            {
                var client = customerManagementApi.Client;

                Trace.WriteLine(Environment.NewLine + "> GetUser: ");
                client.DebugExecute(c => c.GetUser(new GetUserRequest() { }));

                Trace.WriteLine(Environment.NewLine + "> FindAccounts: ");
                client.DebugExecute(c => c.FindAccounts(new FindAccountsRequest() { TopN = 10 }));

                Trace.WriteLine(Environment.NewLine + "> GetAccount: ");
                client.DebugExecute(c => c.GetAccount(new GetAccountRequest() { AccountId = accountId }));
            }

            using (var customerBillingApi = clientFactory.CreateApiClient<ICustomerBillingService>())
            {
                var client = customerBillingApi.Client;

                Trace.WriteLine(Environment.NewLine + "> GetAccountMonthlySpend: ");
                client.DebugExecute(c => c.GetAccountMonthlySpend(
                    new GetAccountMonthlySpendRequest()
                    {
                        AccountId = accountId,
                        MonthYear = DateTime.UtcNow
                    }));
            }

            using (var campaignManagementApi = clientFactory.CreateApiClient<ICampaignManagementService>())
            {
                var client = campaignManagementApi.Client;

                Trace.WriteLine(Environment.NewLine + "> GetCampaignsByAccountId: ");
                client.DebugExecute(c => c.GetCampaignsByAccountId(
                    new GetCampaignsByAccountIdRequest()
                    {
                        //CustomerAccountId = accountId.ToString(), // set in header
                        AccountId = accountId,
                        CampaignType = CampaignType.Search,
                    }));
            }

            using (var bulkApi = clientFactory.CreateApiClient<IBulkService>())
            {
                var client = bulkApi.Client;

                Trace.WriteLine(Environment.NewLine + "> DownloadCampaignsByAccountIds: ");
                client.DebugExecute(c => c.DownloadCampaignsByAccountIds(
                    new DownloadCampaignsByAccountIdsRequest()
                    {
                        //CustomerAccountId = accountId.ToString(), // set in header
                        AccountIds = new long[] { accountId },
                        DataScope = DataScope.EntityData,
                        DownloadEntities = new DownloadEntity[] { DownloadEntity.Campaigns },
                        FormatVersion = "6.0"
                    }));
            }

            using (var reportingApi = clientFactory.CreateApiClient<IReportingService>())
            {
                var client = reportingApi.Client;

                Trace.WriteLine(Environment.NewLine + "> SubmitGenerateReport: ");
                var reportResponse = client.DebugExecute(c => c.SubmitGenerateReport(
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
                    }));

                if (reportResponse != null)
                {
                    Trace.WriteLine(Environment.NewLine + "> PollGenerateReport: ");
                    client.DebugExecute(c => c.PollGenerateReport(
                        new PollGenerateReportRequest()
                        {
                            ReportRequestId = reportResponse.ReportRequestId
                        }));
                }
            }
        }
    }
}