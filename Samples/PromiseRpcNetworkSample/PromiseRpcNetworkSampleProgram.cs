using MarymoorStudios.Core.Promises.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace PromiseRpcNetworkSample;

internal static class PromiseRpcNetworkSampleProgram
{
  private static async Task<int> Main(string[] args)
  {
    using ILoggerFactory loggingFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
     // .SetMinimumLevel(LogLevel.Debug)
     .AddEventSourceLogger()
     .AddSimpleConsole(options =>
      {
        options.ColorBehavior = LoggerColorBehavior.Enabled;
        options.SingleLine = true;
        // ReSharper disable once StringLiteralTypo
        options.TimestampFormat = "[yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff] ";
      }));

    RootCommand rootCommand = new("Promise RPC Stress CLI")
    {
      new HostDemo().CreateCommandGroup(),
      new ClientDemo().CreateCommandGroup(),
    };

    CommandLineBuilder builder = new(rootCommand);
    builder.UseDefaults();
    builder.UseLogging(loggingFactory);
    Parser parser = builder.Build();
    return await parser.InvokeAsync(args);
  }
}
