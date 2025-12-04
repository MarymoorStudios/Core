using MarymoorStudios.Core.Promises.CommandLine;

namespace HsmSample;

[CommandGroup(Name = nameof(Blinker))]
internal sealed class Blinker
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    TrafficLight light = new();

    while (!cancel.IsCancellationRequested)
    {
      // Send a tick event.
      light.Process();
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

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    /// <summary>The current state of the machine.</summary>
    private States m_current = States.Off;

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>A tick event.</summary>
    public void Process()
    {
      switch (m_current)
      {
        case States.Off:
          m_current = States.On; // Transition to a new state.
          Console.WriteLine(m_current);
          break;
        case States.On:
          m_current = States.Off; // Transition to a new state.
          Console.WriteLine(m_current);
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
