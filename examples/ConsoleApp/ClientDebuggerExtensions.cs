using Microsoft.BingAds.SDK.Extensions;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.ServiceModel;

namespace ConsoleApp
{
    public static class ClientDebuggerExtensions
    {
        [DebuggerStepThrough]
        public static TResponse DebugExecute<TClient, TResponse>(this TClient client, Func<TClient, TResponse> clientFunc)
            where TResponse : class
        {
            try
            {
                var response = clientFunc.Invoke(client);
                Trace.WriteLine($"response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (FaultException faultEx)
            {
                Trace.TraceError($"FaultDetail: {JsonConvert.SerializeObject(faultEx.GetFaultDetail(), Formatting.Indented)}");
                return null;
            }
        }
    }
}