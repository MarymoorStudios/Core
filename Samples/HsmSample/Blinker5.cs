using MarymoorStudios.Core.Promises.CommandLine;

#pragma warning disable CA1515
#pragma warning disable CA1822

namespace HsmSample;

[CommandGroup(Name = nameof(Blinker5))]
internal sealed class Blinker5
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    TrafficIntersection light = new();

    DateTime last = DateTime.Now;
    while (!cancel.IsCancellationRequested)
    {
      // Send keyboard events (if any).
      if (Console.KeyAvailable)
      {
        ConsoleKeyInfo key = Console.ReadKey();
        if (key.Key is ConsoleKey.UpArrow or ConsoleKey.DownArrow)
        {
          light.MaintenanceMode(key.Key == ConsoleKey.DownArrow);
        }
      }

      // Send a tick event.
      DateTime next = DateTime.Now;
      light.Process(next - last);
      last = next;
    }
  }

  private sealed class TrafficIntersection
  {
    /// <summary>The amount of time to delay between blinks.</summary>
    private static readonly TimeSpan s_threshold = TimeSpan.FromSeconds(1);

    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    /// <summary>Possible behavior modes of the machine.</summary>
    private enum States
    {
      /// <summary>The intersection is in normal operation.</summary>
      Normal,

      /// <summary>The intersection is in maintenance mode.</summary>
      Maintenance,
    }

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    /// <summary>The current state of the machine.</summary>
    private States m_current = States.Normal;

    private readonly TrafficLight[] m_lights;

    public TrafficIntersection()
    {
      m_lights =
      [
        new TrafficLight(this, TrafficLight.Direction.North, TrafficLight.Color.Yellow),
        new TrafficLight(this, TrafficLight.Direction.East, TrafficLight.Color.Red),
        new TrafficLight(this, TrafficLight.Direction.South, TrafficLight.Color.Yellow),
        new TrafficLight(this, TrafficLight.Direction.West, TrafficLight.Color.Red),
      ];
    }

    public TimeSpan Threshold => s_threshold;
    public bool IsMaintenance => m_current == States.Maintenance;

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>A maintenance input event.</summary>
    public void MaintenanceMode(bool enable)
    {
      m_current = enable ? States.Maintenance : States.Normal;
    }

    /// <summary>A tick event.</summary>
    public void Process(TimeSpan delta)
    {
      foreach (TrafficLight l in m_lights)
      {
        l.Process(delta);
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return m_current.ToString();
    }
  }

  private sealed class TrafficLight
  {
    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    /// <summary>Possible behavior modes of the machine.</summary>
    private enum States
    {
      /// <summary>The light is off.</summary>
      Off,

      /// <summary>The light is on.</summary>
      On,
    }

    /// <summary>The direction the light is facing.</summary>
    public enum Direction
    {
      /// <summary>The light faces north.</summary>
      North,

      /// <summary>The light faces south.</summary>
      South,

      /// <summary>The light faces east.</summary>
      East,

      /// <summary>The light faces west.</summary>
      West,
    }

    public enum Color
    {
      Yellow,
      Red,
    }

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    /// <summary>The intersection this light belongs to.</summary>
    private readonly TrafficIntersection m_parent;

    /// <summary>The direction the light faces.</summary>
    private readonly Direction m_direction;

    /// <summary>The normal color of the light.</summary>
    private readonly Color m_color;

    /// <summary>The current state of the machine.</summary>
    private States m_current = States.Off;

    /// <summary>The time spent in the current state.</summary>
    private TimeSpan m_time = TimeSpan.Zero;

    public TrafficLight(TrafficIntersection parent, Direction direction, Color color)
    {
      m_parent = parent;
      m_direction = direction;
      m_color = color;
    }

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>A tick event.</summary>
    public void Process(TimeSpan delta)
    {
      switch (m_current)
      {
        case States.Off:
          m_time += delta;
          if (m_time > m_parent.Threshold)
          {
            m_time = TimeSpan.Zero;
            m_current = States.On; // Transition to a new state.
            Console.WriteLine($"{m_direction}: Off");
          }
          break;
        case States.On:
          m_time += delta;
          if (m_time > m_parent.Threshold)
          {
            m_time = TimeSpan.Zero;
            m_current = States.Off; // Transition to a new state.
            if (m_parent.IsMaintenance)
            {
              Console.WriteLine($"{m_direction}: {Color.Red}");
            }
            else
            {
              Console.WriteLine($"{m_direction}: {m_color}");
            }
          }
          break;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return m_current.ToString();
    }
  }
}
