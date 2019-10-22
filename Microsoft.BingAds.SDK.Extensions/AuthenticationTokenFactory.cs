using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BingAds.SDK.Extensions
{
    /// <summary>
    /// IAuthenticationTokenFactory
    /// </summary>
    public interface IAuthenticationTokenFactory
    {
        /// <summary>
        /// Creates and returns OAuth AuthenticationToken string. 
        /// This method is expected to cache and return results quick. 
        /// Also, refresh token whenever required, via interactive or otherwise. 
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>OAuth Token</returns>
        Task<string> CreateAuthenticationTokenAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// AuthenticationTokenFactory handles OAuth token generation.
    /// </summary>
    public class AuthenticationTokenFactory : IAuthenticationTokenFactory
    {
        private static object syncRoot = new object();
        private IPublicClientApplication application;
        private AuthenticationResult authenticationResult;
        private ApiEnvironment apiEnvironment;
        private string usernameLoginHint;
        private int acesssTokenExpirationOffsetInSec;

        /// <summary>
        /// ctor for AuthenticationTokenFactory
        /// </summary>
        /// <param name="applicationClientId">ClientId for AAD application that accesses BingAdsApi</param>
        /// <param name="usernameLoginHint">Username LoginHint to retrieve matching AccessToken from TokenCache</param>
        /// <param name="tokenCacheCorrelationId">
        ///   Used to identify persisted TokenCache location. 
        ///   Provide a consistent non-null value across sessions for access to the same persisted cache. 
        ///   If app must not need TokenCache persistence, just pass in null. 
        /// </param>
        /// <param name="acesssTokenExpirationOffsetInSec">
        ///   Specifies when a new Token will be refreshed silently, based on AccessToken's expiresOn property. 
        ///   If AccessToken.ExpiresOn = "09:30" and offset = "-600", 
        ///     then call to fetch accessToken after "09:20" will try to get a refreshed accessToken.
        /// </param>
        /// <param name="apiEnvironment"></param>
        public AuthenticationTokenFactory(
            string applicationClientId,
            string usernameLoginHint = null,
            string tokenCacheCorrelationId = null,
            int acesssTokenExpirationOffsetInSec = Config.DefaultAccessTokenExpirationOffsetInSec,
            ApiEnvironment apiEnvironment = ApiEnvironment.Production)
        {
            if (string.IsNullOrWhiteSpace(applicationClientId))
                throw new ArgumentNullException(nameof(applicationClientId));
            if (acesssTokenExpirationOffsetInSec > 0)
                throw new ArgumentException($"value of {nameof(acesssTokenExpirationOffsetInSec)} must be negative");

            this.apiEnvironment = apiEnvironment;
            this.usernameLoginHint = usernameLoginHint;
            this.acesssTokenExpirationOffsetInSec = acesssTokenExpirationOffsetInSec;

            // create IPublicClientApplication for accessing BingAdsApi
            application = PublicClientApplicationBuilder.Create(applicationClientId)
                .WithRedirectUri(Config.RedirectUri[apiEnvironment])
                .Build();

            // try load TokenCache from persisted file storage
            if (tokenCacheCorrelationId != null)
            {
                var cacheFileName = $".msalcache.{tokenCacheCorrelationId}";
                Logger.Verbose($"TokenCache persistence at {Config.DefaultTokenCacheFileLocation}\\{cacheFileName}");

                var cacheStorageInfo = new StorageCreationPropertiesBuilder(
                    cacheFileName,
                    Config.DefaultTokenCacheFileLocation,
                    applicationClientId)
                    .Build();

                var cacheHelper = MsalCacheHelper.CreateAsync(cacheStorageInfo).Result;

                cacheHelper.RegisterCache(application.UserTokenCache);
            }
        }

        public async Task<string> CreateAuthenticationTokenAsync(CancellationToken cancellationToken)
        {
            // pattern from https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token

            if (TryGetValidAccessToken(out var accessToken))
                return accessToken;

            // find accountLoginHint from executionContext or from usernameLoginHint parameter
            var accountLoginHint = authenticationResult?.Account;
            if (accountLoginHint == null && !string.IsNullOrWhiteSpace(usernameLoginHint))
            {
                var accounts = await application.GetAccountsAsync();
                accountLoginHint = accounts.FirstOrDefault(a => string.Equals(a.Username, usernameLoginHint));
            }

            try
            {
                // try to acquire Token silently
                Logger.Verbose($"AcquireTokenSilent called");
                authenticationResult = await application.AcquireTokenSilent(Config.Scopes[apiEnvironment], accountLoginHint)
                    .ExecuteAsync(cancellationToken);
                return authenticationResult.AccessToken;
            }
            catch (MsalUiRequiredException msalUiEx)
            {
                // control is passed here,
                // - first time app starts, and no TokenCache is persisted. 
                // - if TokenCache is persisted but, 
                //   either usernameLoginHint is not provided or related Token doesn't exist in cache
                // - if AccessToken expires and cannot be refreshed silently. 
                Logger.Error($"exceptionUI: {msalUiEx.Message}");

                try
                {
                    // double-lock to avoid multiple user prompts in parallel threads
                    if (TryGetValidAccessToken(out accessToken))
                        return accessToken;

                    lock (syncRoot)
                    {
                        if (TryGetValidAccessToken(out accessToken))
                            return accessToken;

                        // acquire Token interactively via UI
                        Logger.Verbose($"AcquireTokenInteractive called");
                        authenticationResult = application.AcquireTokenInteractive(Config.Scopes[apiEnvironment])
                            .ExecuteAsync(cancellationToken)
                            .Result;

                        return authenticationResult.AccessToken;
                    }
                }
                catch (MsalException msalEx)
                {
                    Logger.Error($"exception: {msalEx.Message}");
                }
            }

            return null;
        }

        protected bool TryGetValidAccessToken(out string accessToken)
        {
            if (authenticationResult != null
                && DateTime.UtcNow < authenticationResult.ExpiresOn.AddSeconds(acesssTokenExpirationOffsetInSec))
            {
                accessToken = authenticationResult.AccessToken;
                return true;
            }

            accessToken = null;
            return false;
        }
    }
}