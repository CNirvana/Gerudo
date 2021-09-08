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

            Log.Information("Hello, world!");
        }

        internal static void Shutdown()
        {
            Log.CloseAndFlush();
        }

        public static void Debug(string message)
        {
            Log.Debug(message);
        }
    }
}