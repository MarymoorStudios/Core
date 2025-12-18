using MarymoorStudios.Core.Promises.CommandLine;

namespace HsmSample;

[CommandGroup(Name = nameof(LinearWorld))]
internal sealed partial class LinearWorld
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    Console.WriteLine("Welcome to Linear World!");
    Console.WriteLine();

    // Create a scene.
    SceneTree s =
    [
      // Add some weapons.
      new Knife(),
      // Add a player.
      new Player(),
      // Add some enemies.
      new Enemy(),
    ];

    // Run the scene until it terminates.
    using Renderer renderer = new();
    s.Run(renderer, cancel);
  }
}
