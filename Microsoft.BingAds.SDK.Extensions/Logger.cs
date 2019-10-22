using System;
using System.Diagnostics;

namespace Microsoft.BingAds.SDK.Extensions
{
    public static class Logger
    {
        public static Action<string, string, TraceEventType> LogListener { get; set; }
            = (string str, string source, TraceEventType type)
                => Trace.WriteLine($"[{DateTime.UtcNow.ToString("u")}] {type}: {source}: {str}");

        internal static void Error(string str, string source = null)
            => LogListener?.Invoke(str, source ?? Source, TraceEventType.Error);

        internal static void Verbose(string str, string source = null)
            => LogListener?.Invoke(str, source ?? Source, TraceEventType.Verbose);

        private static string Source
        {
            get { var method = new StackFrame(2).GetMethod(); return $"{method.DeclaringType.Name}.{method.Name}"; }
        }
    }
}