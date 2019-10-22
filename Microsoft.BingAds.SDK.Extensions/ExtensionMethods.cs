using System.Linq;
using System.ServiceModel;

namespace Microsoft.BingAds.SDK.Extensions
{
    public static class ExtensionMethods
    {
        public static object GetFaultDetail(this FaultException faultEx)
            => faultEx.GetType().GetProperties().FirstOrDefault(p => p.Name == "Detail")?.GetValue(faultEx, null);
    }
}