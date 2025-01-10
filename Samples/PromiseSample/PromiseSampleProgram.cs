using MarymoorStudios.Core.Promises;

namespace PromiseSample;

internal static class PromiseSampleProgram
{
  private static int Main(string[] args)
  {
    // Promise computations run in single-threaded containers called a "Software Isolated Process" (or "Sip").
    // 
    // The `Sip` class provides convenient static overloads for creating the root Sip for a process with a variety of 
    // possible arguments and return types.
    return Sip.CreateAndRun(args,
      // The lambda passed here is of type `Func<TArg, Promise<TReturn>>`.  The Sip created here will run until
      // the `Promise<TReturn>` _resolves_.  The resolution may be an asynchronous operation (as it is here) allowing
      // other work to be performed concurrently.
      async args2 =>
      {
        long number = long.Parse(args2[0]);
        // Perform any number of asynchronous computations either using `await` or `Promise` operations or any mix.
        long result = await ComputeFibonacci(number);
        Console.WriteLine($"Fibonacci({number}) = {result}");
        return 0;
      });
  }

  // `Promise` and `Promise<T>` are drop-in replacements for `Task` and `Task<T>` with respect to the `await`
  // and `async` keywords.  This defines a `Promise`-based async method.
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
