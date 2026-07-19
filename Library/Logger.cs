using System;
using System.IO;

internal static class Logger
{
    private static readonly object _lock = new object();
    private static readonly string _path = Path.Combine(Path.GetTempPath(), "vnebo.mobi.bot.log");

    public static void Write(string message)
    {
        try
        {
            lock (_lock)
            {
                File.AppendAllText(_path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
        }
        catch { /* ignore logging failures */ }
#if DEBUG
        try { Console.WriteLine(message); } catch { }
#endif
    }

    public static void WriteError(string message) => Write("ERROR: " + message);
}
