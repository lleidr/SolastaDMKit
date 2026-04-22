using System;

namespace SolastaDMKit.Core.Diagnostics;

public static class SxLog
{
    public static Action<string> InfoCallback { get; set; } = _ => { };
    public static Action<string, Exception> ErrorCallback { get; set; } = (_, _) => { };

    public static void Info(string message) => InfoCallback(message);
    public static void Error(string message, Exception ex = null) => ErrorCallback(message, ex);
}
