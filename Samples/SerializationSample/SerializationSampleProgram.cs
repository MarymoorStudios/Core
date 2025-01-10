
using MarymoorStudios.Core.Serialization;

namespace SerializationSample;

internal sealed partial class SerializationSampleProgram
{
  private static void Main()
  {
    // Create a sample data contract value.
    SampleDataRecord data = new(42, "This is a sample.");

    // Serialize the data contract to a byte sequence.
    ReadOnlyMemory<byte> bytes = DataContractSerializer.Serialize(data);
    Console.WriteLine($"Length: {bytes.Length} Bytes: {Convert.ToHexString(bytes.Span)}\n");

    // Deserialize the data contract from the byte sequence to new value.
    if (!DataContractSerializer.TryDeserialize(bytes, out SampleDataRecord data2))
    {
      Console.WriteLine("ERROR: Could not deserialize the value");
    }
    else
    {
      Console.WriteLine($"Result: {data2}\n");
    }
  }

  // Define a custom data contract type (using `class`, `struct`, `record`, or `record struct`).
  // 
  // DataContract types can be public, internal, or private.  When private their enclosing types MUST be defined as
  // `partial` to allow the code generator to embed the appropriate serialization logic.  The generated serialization
  // logic will have the same visibility as the data contract type it serializes.
  //
  // All data contract types MUST have a public constructor that takes ALL data members in the same order they were
  // declared.  When using `record` or `record struct` the compiler generates such a constructor automatically.
  //
  // Note the attributes used to define a data contract are `MarymoorStudios.Core.Serialization.DataContractAttribute`
  // and `MarymoorStudios.Core.Serialization.DataMemberAttribute` in the `MarymoorStudios.Core.Serialization` namespace
  // not those in the `System.Runtime.Serialization` namespace.
  [DataContract]
  private record struct SampleDataRecord(int Value, string Label);
}
