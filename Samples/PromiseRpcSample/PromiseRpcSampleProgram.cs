// ReSharper disable UnusedType.Local

using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Rpc;

namespace PromiseRpcSample;

internal static partial class PromiseRpcSampleProgram
{
  private static void Main()
  {
    Sip.CreateAndRun(async () =>
    {
      // Create an object.
      CalcProxy c1 = new(new Calc());

      // Objects are stateful.
      Console.WriteLine($"init => {await c1.Get()}");
      Console.WriteLine($"c1.Add(7) => {await c1.Add(7)}");
      Console.WriteLine($"c1.Add(8) => {await c1.Add(8)}");

      // Objects have identity and independent lifetime.  
      // Create two objects and show they have distinct state.
      CalcProxy c2 = c1.Clone();
      Console.WriteLine($"c1.Clone() => {await c2.Get()}");
      Console.WriteLine($"c1.Add(3) => {await c1.Add(3)}");
      Console.WriteLine($"c1.Get() => {await c1.Get()}");
      Console.WriteLine($"c2.Get() => {await c2.Get()}");

      // Object support pipelining (issuing multiple calls concurrently).
      // Object references (proxies) maintain Linear Dispatch Order (method happen in the order they were called).
      Promise p1 = c1.Clear();
      Promise<long> p2 = c1.Add(6);
      // Two calls were issued without awaiting - they are sent concurrently.
      // Await for p2 to finish.  Methods can resolve (their return value becomes available) out-of-order.
      long n = await p2;
      // Given that the `Clear` and the `Add(6)` were sent in parallel, what are the possible values for `n`?
      // Will it be `6`?  Or `24` (`18` the value of the c1 register before the `Clear` + `6`)?  Or either of those
      // non-deterministically?
      //
      // Even though the method calls were pipelined they are guaranteed to be dispatched at the object in linear
      // dispatch order so the `Clear` is guaranteed to be dispatched *before* the `Add(6)` thus deterministically
      // producing `6`.
      //
      // Linear Dispatch Ordering is uncommon in RPC systems, but makes distributed programming considerably easier.
      // Without it the `Clear` should NOT be pipelined and instead the caller would have to await for it to complete
      // before issuing the call to `Add(6)`.  When the transmission time is long (say over a slow network link) this
      // extra roundtrip time can add up to long delays.
      Console.WriteLine($"c1 Pipeline (Clear, Add(6)) => {n}");

      // P1 will also resolve at some point.
      await p1;
    });
  }

  /// <summary>Define an eventual interface for a simple calculator object with one register as state.</summary>
  [Eventual]
  private interface ICalc
  {
    /// <summary>Returns the current value of the register.</summary>
    /// <remarks>Note: doc-comments are tunneled from the interface definition to the generated code.</remarks>
    public Promise<long> Get();

    /// <summary>Clear the register.</summary>
    public Promise Clear();

    /// <summary>Add a value to the register.</summary>
    /// <param name="n">the value to add.</param>
    /// <returns>The contents of the register after the operation completes</returns>
    public Promise<long> Add(long n);

    /// <summary>Add a value to the register.</summary>
    /// <param name="c">a calculator whose register value to add to this object's register.</param>
    /// <returns>The contents of the register after the operation completes</returns>
    public Promise<long> AddCalc(CalcProxy c);

    /// <summary>Create a new independent calculator whose register is initialized to this object's register.</summary>
    /// <returns>The new object.</returns>
    public CalcProxy Clone();
  }

  /// <summary>Implement the calculator eventual interface.</summary>
  private sealed class Calc : CalcServer
  {
    /// <summary>The single register state.</summary>
    private long m_register;

    public Calc()
    {
      m_register = 0;
    }

    /// <inheritdoc/>
    public override Promise<long> Get()
    {
      return Promise.From(m_register);
    }

    /// <inheritdoc/>
    public override Promise Clear()
    {
      m_register = 0;
      return Promise.Done;
    }

    /// <inheritdoc/>
    public override Promise<long> Add(long n)
    {
      m_register += n;
      return Promise.From(m_register);
    }

    /// <inheritdoc/>
    public override async Promise<long> AddCalc(CalcProxy c)
    {
      long n = await c.Get();
      return await Add(n);
    }

    /// <inheritdoc/>
    public override CalcProxy Clone()
    {
      Calc clone = new()
      {
        m_register = m_register,
      };
      return new CalcProxy(clone);
    }
  }
}
