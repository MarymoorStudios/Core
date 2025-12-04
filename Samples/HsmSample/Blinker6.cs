using MarymoorStudios.Core.Fsm;
using MarymoorStudios.Core.Promises.CommandLine;

#pragma warning disable CA1515
#pragma warning disable CA1822

namespace HsmSample;

[CommandGroup(Name = nameof(Blinker6))]
internal sealed partial class Blinker6
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

  [Hsm]
  private sealed partial class TrafficIntersection
  {
    /// <summary>The amount of time to delay between blinks.</summary>
    private static readonly TimeSpan s_threshold = TimeSpan.FromSeconds(1);

    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    [HsmStates]
    private static void StateMachine(
      HsmMeta<TrafficIntersection, Hsm, State, Inputs> m,
      Inputs i,
      TrafficIntersection x
    )
    {
      m.Composite(State.Init,
          [m.State(State.Normal), m.State(State.Maintenance)])
       .OnStart(State.Normal)
       .Edge(i.MaintenanceMode, x.OnMaintenanceMode, State.Normal, State.Maintenance)
       .Edge(i.Process, x.OnProcess);
    }

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    private readonly Hsm m_state;

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
      m_state = Hsm.Create(this, StateMachine);
    }

    public TimeSpan Threshold => s_threshold;
    public bool IsMaintenance => ReferenceEquals(m_state.CurrentState, State.Maintenance);

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    [HsmInput]
    public void MaintenanceMode(bool enable)
    {
      m_state.MaintenanceMode(this, enable);
    }

    [HsmInput]
    public void Process(TimeSpan delta)
    {
      m_state.Process(this, delta);
    }

    /// <summary>A maintenance input event.</summary>
    private void OnMaintenanceMode(Hsm m, bool enable)
    {
      m.SetNext(enable ? State.Maintenance : State.Normal);
    }

    /// <summary>A tick event.</summary>
    private void OnProcess(Hsm m, TimeSpan delta)
    {
      foreach (TrafficLight l in m_lights)
      {
        l.Process(delta);
      }
    }
  }

  [Hsm]
  private sealed partial class TrafficLight
  {
    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    [HsmStates]
    private static void StateMachine(HsmMeta<TrafficLight, Hsm, State, Inputs> m, Inputs i, TrafficLight x)
    {
      m.Composite(State.Init,
        [
          m.State(State.Off)
           .Edge(i.Process, x.OnProcessOff, State.Off, State.On),
          m.State(State.On)
           .Edge(i.Process, x.OnProcessOn, State.Off, State.On),
        ])
       .OnStart(State.Off);
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
    private readonly Hsm m_state;

    /// <summary>The time spent in the current state.</summary>
    private TimeSpan m_time = TimeSpan.Zero;

    public TrafficLight(TrafficIntersection parent, Direction direction, Color color)
    {
      m_parent = parent;
      m_direction = direction;
      m_color = color;
      m_state = Hsm.Create(this, StateMachine);
    }

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>A tick event.</summary>
    [HsmInput]
    public void Process(TimeSpan delta)
    {
      m_state.Process(this, delta);
    }

    private void OnProcessOff(Hsm m, TimeSpan delta)
    {
      m_time += delta;
      if (m_time > m_parent.Threshold)
      {
        m_time = TimeSpan.Zero;
        m.SetNext(State.On); // Transition to a new state.
        Console.WriteLine($"{m_direction}: Off");
      }
    }

    private void OnProcessOn(Hsm m, TimeSpan delta)
    {
      m_time += delta;
      if (m_time > m_parent.Threshold)
      {
        m_time = TimeSpan.Zero;
        m.SetNext(State.Off); // Transition to a new state.
        if (m_parent.IsMaintenance)
        {
          Console.WriteLine($"{m_direction}: {Color.Red}");
        }
        else
        {
          Console.WriteLine($"{m_direction}: {m_color}");
        }
      }
    }
  }
}
