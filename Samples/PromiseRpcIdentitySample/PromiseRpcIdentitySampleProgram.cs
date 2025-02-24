using MarymoorStudios.Core.Promises.CommandLine;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace PromiseRpcIdentitySample;

internal static class PromiseRpcIdentitySampleProgram
{
  private static async Task<int> Main(string[] args)
  {
    using ILoggerFactory loggingFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
     .AddEventSourceLogger());

    RootCommand rootCommand = new("Promise RPC Identity Sample")
    {
      new HostDemo().CreateCommandGroup(),
      new ClientDemo().CreateCommandGroup(),
    };

    CommandLineBuilder builder = new(rootCommand);
    builder.UseDefaults();
    builder.UseLogging(loggingFactory);
    builder.UseMarymoorAuthentication();
    Parser parser = builder.Build();
    return await parser.InvokeAsync(args);
  }
}
