using Microsoft.Extensions.Configuration;
using System.Security;

namespace MyNotifier.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var commandLineArgs = ParseCommandLineArgs(args);

            var configuration = new ConfigurationBuilder().AddJsonFile(commandLineArgs.AppsettingsPath).Build();

            await new Driver(configuration).DriveAsync(commandLineArgs.SessionKey).ConfigureAwait(false);
        }

        private static CommandLineArgs ParseCommandLineArgs(string[] args) { return new CommandLineArgs(); }
        private class CommandLineArgs
        {
            public string AppsettingsPath { get; set; }
            public SecureString SessionKey { get; set; }
        }
    }
}
