using Serilog;

namespace Gerudo
{
    public static class Logger
    {
        internal static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("log.log")
                .CreateLogger();
        }

        internal static void Shutdown()
        {
            Log.CloseAndFlush();
        }

        public static void LogInfo(string message)
        {
            Log.Debug(message);
        }

        public static void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public static void LogError(string message)
        {
            Log.Error(message);
        }

        public static void LogFatal(string message)
        {
            Log.Fatal(message);
        }
    }
}