using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace HsmSample;

internal static class HsmSampleProgram
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

    RootCommand rootCommand = new("HSM Sample")
    {
      new Blinker().CreateCommandGroup(),
      new Blinker2().CreateCommandGroup(),
      new Blinker3().CreateCommandGroup(),
      new Blinker4().CreateCommandGroup(),
      new Blinker5().CreateCommandGroup(),
      new Blinker6().CreateCommandGroup(),
      new Hero().CreateCommandGroup(),
      new Hero2().CreateCommandGroup(),
      new LinearWorld().CreateCommandGroup(),
    };

    ParseResult parseResult = rootCommand.Parse(args);
    return await parseResult.InvokeAsync();
  }
}
