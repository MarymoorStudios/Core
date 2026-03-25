using MarymoorStudios.Core.Promises.CommandLine;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace PromiseRpcIdentitySample;

internal static class PromiseRpcIdentitySampleProgram
{
  private static async Task<int> Main(string[] args)
  {
    using ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
       .SetMinimumLevel(LogLevel.Debug)
        // DEVELOPER NOTE: Try out https://www.nuget.org/packages/MarymoorStudios.Core.Rpc.TraceCli
       .AddEventSourceLogger()
      // DEVELOPER NOTE: Uncomment the following lines to see logging in the console window.
      //.AddSimpleConsole(options =>
      // {
      //   options.ColorBehavior = LoggerColorBehavior.Enabled;
      //   options.SingleLine = true;
      //   // ReSharper disable once StringLiteralTypo
      //   options.TimestampFormat = "[yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff] ";
      // })
    );

    RootCommand rootCommand = new("Promise RPC Identity Sample")
    {
      new HostDemo().CreateCommandGroup(loggerFactory),
      new ClientDemo().CreateCommandGroup(loggerFactory),
    };
    rootCommand.UseMarymoorAuthentication();

    ParseResult parseResult = rootCommand.Parse(args);
    return await parseResult.InvokeAsync();
  }
}
