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