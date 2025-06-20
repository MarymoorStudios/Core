using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises;
using MarymoorStudios.Core.Rpc;
using MarymoorStudios.Core.Rpc.Exceptions;
using System.Diagnostics;

namespace SteamRpcNetworkSample;

/// <summary>An implementation of <see cref="DemoServer"/> for the Host sip.</summary>
[DebuggerDisplay("{m_id}")]
internal sealed class HostDemoServer : DemoServer
{
  /// <summary>A unique identifier for this turn.</summary>
  private readonly Guid m_id;

  private readonly DemoProxy m_self;

  public HostDemoServer()
  {
    m_id = Guid.NewGuid();
    m_self = new DemoProxy(this);
  }

  /// <inheritdoc/>
  public override Promise<Guid> GetId()
  {
    return new Promise<Guid>(m_id);
  }

  /// <inheritdoc/>
  public override async Promise<string> Echo(
    string message,
    Guid tag = default,
    TimeSpan delay = default,
    bool shouldThrow = false
  )
  {
    Console.WriteLine("Echo: Begin {0} {1} {2}", tag, message.Length, delay);
    Contract.Requires(delay >= TimeSpan.Zero);

    await Scheduler.Delay(delay);

    if (shouldThrow)
    {
      throw new TestException(message);
    }

    Console.WriteLine("Echo: End {0} {1} {2}", tag, message.Length, delay);
    return message;
  }

  /// <inheritdoc/>
  public override DemoProxy GetSelf()
  {
    return m_self;
  }

  /// <inheritdoc/>
  public override Promise SendPromise(Promise p, Guid tag = default, TimeSpan delay = default, bool shouldWait = true)
  {
    Console.WriteLine("SendPromise: Begin Void {0} {1} {2}", tag, delay, shouldWait);
    Contract.Requires(delay >= TimeSpan.Zero);

    // If prompt, then just forward the result without any waiting.
    if (delay == TimeSpan.Zero && !shouldWait)
    {
      return p;
    }

    // Delay...
    return Scheduler.Delay(delay)
     .When(() =>
      {
        // ... then either forward or wait.
        if (shouldWait)
        {
          return p.When(() => { Console.WriteLine("SendPromise: End Void {0} {1} {2}", tag, delay, shouldWait); });
        }

        // Forward.
        Console.WriteLine("SendPromise: End Void {0} {1} {2}", tag, delay, shouldWait);
        return p;
      });
  }

  /// <inheritdoc/>
  public override Promise<int> SendPromise(
    Promise<int> p,
    Guid tag = default,
    TimeSpan delay = default,
    bool shouldWait = true
  )
  {
    Console.WriteLine("SendPromise: Begin Data {0} {1} {2}", tag, delay, shouldWait);
    Contract.Requires(delay >= TimeSpan.Zero);

    // If prompt, then just forward the result without any waiting.
    if (delay == TimeSpan.Zero && !shouldWait)
    {
      return p;
    }

    // Delay...
    return Scheduler.Delay(delay)
     .When(() =>
      {
        // ... then either forward or wait.
        if (shouldWait)
        {
          return p.When(x =>
          {
            Console.WriteLine("SendPromise: End Data {0} {1} {2}", tag, delay, shouldWait);
            return x;
          });
        }

        // Forward.
        Console.WriteLine("SendPromise: End Data {0} {1} {2}", tag, delay, shouldWait);
        return p;
      });
  }

  /// <inheritdoc/>
  public override DemoProxy SendProxy(DemoProxy p, Guid tag = default, TimeSpan delay = default)
  {
    Console.WriteLine("SendProxy: Begin {0} {1}", tag, delay);

    // Simulate some computation time.
    if (delay > TimeSpan.Zero)
    {
      return Scheduler.Delay(delay)
       .When<DemoProxy, DemoServer>(() =>
        {
          Console.WriteLine("SendProxy: End {0} {1}", tag, delay);
          return p;
        });
    }

    Console.WriteLine("SendProxy: End {0} {1}", tag, delay);
    return p;
  }
}
