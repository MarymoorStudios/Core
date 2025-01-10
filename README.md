# ![Logo](Images/Marymoor%20Studios%20Logo%20NM%2064x64.png) Marymoor Studios Core Libraries 

The Marymoor Studios Core Libraries are a set of tools for developing concurrent and distributed applications.

## Contact and Pricing
Explore license purchase options at the [Marymoor Studios Online Store](https://marymoorstudios.square.site/). Or, 
contact Marymoor Studios, LLC at info@marymoorstudios.com to inquire about additional license pricing and purchase
options.  Both **free** non-commercial use licenses, and paid commercial licenses are available.

## The Core Libraries
The Core Libraries are a suite of C# tools offering a unique, easy to use, correct-by-construction, approach to
race-free concurrent programming.  Inspired by Midori, E-lang and Erlang, the Core is a Promise-based, single-threaded 
software-isolated process (Sip) model loosely conforming to Tony Hoarse's Communicating Sequential Processes (CSP)
formalism.

### Data Contracts:
* Formal structured data contracts.
* Support for all C# primitives (including `byte arrays`, `DateTime`, and `unsigned` values).
* User Defined Types (UDTs) via attributed `class`, `struct`, `record`, or `record struct` types.
* Support for inheritance, collections, and enumerables.
* Support for polymorphic serialization and deserialization.
* Support for generic types.
* Cross-versioning support through partial materialization.
* Custom Serialization extensibility points (for 3rd party types).

```cs
  [DataContract]
  public abstract record GameEvent();

  [DataContract]
  public sealed record BuildCreate(PlayerToken Token, Vector2I Target) : GameEvent;

  [DataContract]
  public sealed record BuildDestroy(PlayerToken Token, Vector2I Target) : GameEvent;
```

### Promise-based Computational Model:
* `Promise`/`Promise<T>` as a drop-in replacement for `Task`/`Task<T>`.
  * Full support for `async` and `await` keywords.
* Single-threaded continuation scheduling:
  * **Naturally Lock-Free**: Eliminates the need for locks or mutexes.
  * Great for **Game Development** (e.g. _Unity_ or _Godot_) because continuation scheduling happens directly on the
    event-loop thread between frames.  No races and can directly interact with game engine objects.
  * **Race-Free Parallelism**: Create multiple _Sips_ to leverage multi-core parallism.
    * Both concurrent and parallel workloads are supported. 
    * Always race-free with efficient, structured, cross-Sip, message-passing based communication.
* `Sequence<T>` provides efficient bulk transfer, subscriptions, and transformations.
  * `IAsyncObservable`-style event-driven programming with server-push support.
  * Fully integrated into the RPC system as a first class data type.
* `Resolver<T>` supports ad-hoc as well as structured concurrency.

```cs
      int count = 0;
      // Add-hoc resolution with first-class resolvers.
      Resolver<Void> r1 = new();

      // Promise-based computations and composition.
      Promise p1 = new(r1);
      Promise p2 = p1.When(() => count++);
      // Promise chaining.
      p2 = p2.When(() => count++);

      // Explicit resolution.
      r1.Resolve();

      // Await for resolutions to settle.
      await p1;
      // Downstream propagation through the computational graph.
      await p2;

      // Functional data-flow
      Promise<int> p3 = p1.When(() => Promise.From(count));
      Assert.AreEqual(2, await p3);
```

### Formal Eventual Interface Specifications:
* Remotable stateful objects accessed remotely by reference (via dynamically created **proxy**).
* Automatic lifetime management between Sips (x-thread, x-process, x-machine).
* Capability-based security model.
* Method specifications formalize wire contracts (i.e. explicit application protocols).
* **Flexible Modelling**: Methods can pass or return:
  * Any **data contract** including both primitives and UDTs.
  * Any remotable **proxy**.  Allows methods to pass references to new objects!
  * Any `Sequence<T>`.  Allows bulks transfers, server push, object streaming (with flow control).
  * A `Bytes` Stream.  Allows efficient binary streaming (with flow control).

```cs
[Eventual]
public partial interface IGameLauncher
{
  /// <summary>The game endpoint.</summary>
  public Promise<IPEndPoint> GetEndpoint();

  /// <summary>Returns the first game event whose timestamp is larger than <paramref name="previous"/>.</summary>
  /// <param name="previous">The timestamp of the last game event seen by the caller.</param>
  /// <returns>The game event along with its timestamp.</returns>
  public Sequence<Timestamped<LauncherEvent>> GetNext(ulong previous);

  /// <summary>Attempts to join the game as a player.</summary>
  /// <param name="player">Which player slot to join as.</param>
  /// <param name="name">The name.</param>
  /// <param name="client">A proxy to the player's client.</param>
  /// <returns>Resolves if successfully, broken otherwise.</returns>
  public JoinedPlayerProxy TryJoin(PlayerToken player, string name, ClientLauncherProxy client);


  ////////////////////////////////////////////////////////////////////////////////////////////////////////////
  // Events
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////

  [DataContract]
  public abstract record LauncherEvent();

  /// <summary>The last event indicating the launcher has launched.</summary>
  [DataContract]
  public sealed record LaunchedEvent() : LauncherEvent;
}

/// <summary>A capability to manage a joined player's slot subscription on an active launcher.</summary>
[Eventual]
public partial interface IJoinedPlayer
{
  /// <summary>Removes the player from the game.</summary>
  public Promise Leave();
}
```

### Roslyn Intgegration
* All features are fully integrated with the Roslyn toolchain and Visual Studio.
* Dynamic real-time code-gen as you type.
* Best-practice analyzers and diagnostics.

## Links
* [Documentation](https://github.com/MarymoorStudios/Core)
* [License](https://github.com/MarymoorStudios/Core/blob/main/LICENSE.md)
