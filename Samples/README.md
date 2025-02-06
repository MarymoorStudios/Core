# ![Logo](../Images/Marymoor%20Studios%20Logo%20NM%2064x64.png) Marymoor Studios Core Samples 

This directory contains a set of samples that demonstrate the use of some of the packages in the Marymoor Studios 
Core Libraries.

All of the samples are independent and can be taken in any order.  The suggested order below introduces lower-level 
concepts first.  Subsequent samples then build on the knowledge of previous samples.

## SerializationSample
Demonstrates the basic use of the `MarymoorStudios.Core.Serialization` package to define data contract types.  Data
contracts are the basis of all structed IO in the Core libraries including local asynchronous operations, local and
remote Promise-based RPC, and structured file IO.

This sample also introduces the package `MarymoorStudios.Core.Generators` which contains all of the Roslyn components
for the Core libraries including Roslyn Analyzers, Diagnostics, and Code-Gen components.  This package should also be
included in any project that requires code-gen.  The serialization and deserialization logic for data contract
types are emitted by code-gen.

## PromiseSample
Demonstrates `MarymoorStudios.Core.Promises` and the foundational concepts around `Promise`, `Promise<T>`, `Resolver<T>`
and `Sip` (aka _Software Isolated Processes_).

## PromiseCommandLineSample
Demonstrates `MarymoorStudios.Core.Promises.CommandLine` which extends `System.CommandLine` to support promise-based
computations.  This package is totally optional, and is provided only as a convenience for those that already use
`System.CommandLine`.

The package `MarymoorStudios.Core.Generators` is also required.  Command group registration logic is emitted by
code-gen.

## PromiseRpcSample
Demonstrates `MarymoorStudios.Core.Rpc` and the foundational concepts of Eventual Interfaces, object implementations, 
proxy references, method pipelining, and Linear Dispatch Order.

The package `MarymoorStudios.Core.Generators` is also required.  Eventual Interface marshalling logic is emitted by
code-gen.

## PromiseRpcNetworkSample
A more complex end-to-end sample that demonstrates `MarymoorStudios.Core.Rpc.Net` as well as more advanced features
of `MarymoorStudios.Core.Rpc` and `MarymoorStudios.Core.Promises` and `MarymoorStudios.Core.Promises.CommandLine`.
This sample implements both a network client and a network host (as two separate CommandLine commands).

The host exports a full remotable demo object that conforms to the Eventual specification defined in `IDemo.cs`.
The implementation of this specification in `HostDemoServer.cs` demonstrates both simple `async` and advanced
`Promise`-based techniques for implementing server-side remotable object methods.  The host continues listening for
incoming client connections until the user hits Ctrl+C.  Try it:

```
dotnet run host run --endpoint 127.0.0.1:8888
```

The client connects to a running host and uses Promises RPC to perform a series of complex interactions with the host
by calling its `Promise`-based RPC methods.  The client keeps executing random actions until the user hits Ctrl+C.  Try
it:

```
dotnet run client run --seed 1 --endpoint 127.0.0.1:8888
```

## CrossVersionV1Sample / CrossVersionV2Sample
These two samples extend [PromiseRpcNetworkSample](#promiserpcnetworksample).  The two samples work together to
demonstrate some advanced techniques for versioning services over time.

### V1 Service - Getting Started with Custom Metadata
The V1 version of this sample introduces (like [PromiseRpcNetworkSample](#promiserpcnetworksample) above) a 
client and a server that exposes a simple interface [`ICross`](CrossVersionV1Sample\ICross.cs) with a single method that
takes and returns a custom data contract called `CrossValue`.  Both the client and the server print to the console the
entire data contract value they receive.  

V1 also demonstrates how to utilize the builtin Eventual Interface `ICapabilities` through the RPC library classes 
`MetadataPublisher` and `IMetadata.Descriptor` to define and publish extensible service metadata.  The V1 service
publishes the custom metadata item `CrossDescriptor` which contains a proxy for an `ICross` object.

Try out the server:

```
dotnet run host run --endpoint 127.0.0.1:8888
```

And then connect with the client:

```
dotnet run client run --endpoint 127.0.0.1:8888
```

### V2 Service - Evolving a Service
The V2 version of this sample evolves the service described above using _additive evolution_ in THREE backward
compatible ways:

1. It extends the `CrossValue` data contract to take an additional property value (`CrossValue.Code`).
2. It defines, implements, and exports a second Eventual Interface called `IExtraCross` with a new method `CallExtra`.
3. It exports additional metadata about this second interface via the custom metdata item `ExtraCrossDescriptor`.

The V2 client, like the V1 client, reads metadata from the service it connects to.  When connected to a V1 server, the
client ONLY does what the V1 client did (i.e. it calls `ICross.Call`), but in doing so it _also_ passes an evolved 
`CrossValue` with the **extra** field.  The V1 service, being **forward compatible to data contract evolution**, will
ignore these extra fields and return the same value is always did.  From the console output you can see both that the
extra field is passed by the V2 client, and that the V1 server ignores it.  The V2 client is compatible with both the V1
and V2 servers.

When connecting to the V2 server, the V2 client will see the additional metadata for the `IExtraCross` interface and
will then dynamically **also** make a call to the `CallExtra` method.  This call is *only* made by a V2/V2 pairing. The
V1 client can still connect normally with the V2 server, but it will not call `CallExtra` because it doesn't know the
`ExtraCrossDescriptor` custom metadata item.  The V2 server is compatible with both the V1 and V2 clients.

### Additive Evolution
In _additive evolution_ you can evolve an existing service in-place in a backward compatible way.  Here are some DOs and 
DON'Ts to ensure backward compatibility:

#### DOs:
1. Add new fields *at the end* of existing data contract types.
2. Make the type of an existing field nullable (that wasn't previously nullable).
3. Add new methods to existing Eventual Interfaces.
4. Add new Eventual Interfaces.
5. Add and publish new custom metadata items (by extending `IMetadata.Descriptor`).

#### DON'T:
1. Add new fields in the middle of existing data contract types.  **DC serialization is strongly ordered.**
    1. Instead add new fields _at the end_.
2. Change the physical type of an existing field.  **DC serialization is strongly typed.**
    1. Instead add new fields _at the end_ (and consider making obsolete fields nullable, if they aren't already).
3. Delete existing fields or change their order.
    1. Instead consider making obsolete fields nullable, if they aren't already.
4. Delete existing methods or change their parameters.  
    1. Interface methods are **unique based on their name + parameter types and order**.
    2. Altering their parameters' types or their order _creates a new method_.
    3. Deleting a method will make it inaccessible to older clients still using it.
    4. Instead, use method overloads or add new methods.
