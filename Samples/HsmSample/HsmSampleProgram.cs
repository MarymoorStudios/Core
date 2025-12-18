using MarymoorStudios.Core.Promises.CommandLine;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace HsmSample;

internal static class HsmSampleProgram
{
  private static async Task<int> Main(string[] args)
  {
    using ILoggerFactory loggingFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
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

    CommandLineBuilder builder = new(rootCommand);
    builder.UseDefaults();
    builder.UseLogging(loggingFactory);
    Parser parser = builder.Build();
    return await parser.InvokeAsync(args);
  }
}
