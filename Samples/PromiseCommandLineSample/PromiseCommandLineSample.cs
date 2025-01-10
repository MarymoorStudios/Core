using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Promises.CommandLine;
using System.CommandLine;

namespace PromiseCommandLineSample;

internal static class PromiseCommandLineSample
{
  private static async Task<int> Main(string[] args)
  {
    RootCommand rootCommand = new("Promise System.CommandLine Sample")
    {
      // The `CreateCommandGroup` method is emitted automatically by the package `MarymoorStudios.Core.Generators`.
      new SampleCommands().CreateCommandGroup(),
    };

    return await rootCommand.InvokeAsync(args);
  }
}

// The `MarymoorStudios.Core.Promises.CommandLine` package extends `System.CommandLine` to support Promise-based
// commands.  The package also automatically creates a root Sip to host the execution of those commands.
//
// The `CommandGroup` attribute marks a class that defines a subgroup of commands.
[CommandGroup("Sample Subgroup", "sample")]
internal sealed class SampleCommands
{
  // The `Command` attribute identifies a method that is a command within the subgroup.
  [Command("Hello World Promise Test", "hello")]
  public static Promise<int> HelloWorld(
    // Commands can take arguments or options.
    [Argument("The message to print")] string message,
    [Option("Echo the output twice", Name = "echo")]
    bool echo
  )
  {
    Console.WriteLine(message);
    if (echo)
    {
      Console.WriteLine(message);
    }
    return new Promise<int>(0);
  }

  [Command("Fibonacci Promise Test", "fib")]
  public static async Promise<int> Fibonacci(
    [Argument(Name = "n", Description = "The argument to compute fib(n)")]
    long number
  )
  {
    long result = await ComputeFibonacci(number);
    Console.WriteLine($"Fibonacci({number}) = {result}");
    return 0;
  }

  private static async Promise<long> ComputeFibonacci(long n)
  {
    return n switch
    {
      0 => 0,
      1 => 1,
      var _ => await ComputeFibonacci(n - 1) + await ComputeFibonacci(n - 2),
    };
  }
}
