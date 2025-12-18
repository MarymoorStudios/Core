using MarymoorStudios.Core;
using MarymoorStudios.Core.Promises.CommandLine;
using System.Numerics;

namespace HsmSample;

[CommandGroup(Name = nameof(Hero))]
internal sealed class Hero
{
  [Command]
  public static void Run(CancellationToken cancel)
  {
    HeroHsm h = new();
    Enemy e = new();

    DateTime last = DateTime.Now;
    while (!cancel.IsCancellationRequested)
    {
      // Send keyboard events (if any).
      if (Console.KeyAvailable)
      {
        ConsoleKeyInfo key = Console.ReadKey();
        switch (key.Key)
        {
          case ConsoleKey.UpArrow:
            h.Jump();
            break;
          case ConsoleKey.DownArrow:
            h.Slide();
            break;
          case ConsoleKey.RightArrow:
            h.Attack(e);
            break;
          case ConsoleKey.LeftArrow:
            h.TakeDamage(10);
            break;
        }
      }

      // Send a tick event.
      DateTime next = DateTime.Now;
      h.Process(next - last);
      last = next;
    }
  }

  private sealed class HeroHsm
  {
    /// <summary>The number of hit points of damaged caused by the hero's weapon.</summary>
    private const int s_attackDamage = 10;

    /// <summary>The force of a jump.</summary>
    private static readonly Vector2 s_jumpVelocity = new(0, 30);

    /// <summary>The force of a slide.</summary>
    private static readonly Vector2 s_slideDisplacement = new(20, 0);

    /// <summary>Default acceleration of gravity.</summary>
    private static readonly Vector2 s_gravity = new(0, -10);

    /// <summary>The size of the play area.</summary>
    private static readonly Vector2 s_screenSize = new(100, 100);

    ////////////////////////////////////////////////////////////////////////
    // Define States
    ////////////////////////////////////////////////////////////////////////

    private static class State
    {
      public static readonly States Ground = new GroundState();
      public static readonly States Airborne = new AirborneState();
      public static readonly States Flying = new FlyingState();
      public static readonly States Downed = new DownedState();
    }

    private abstract class States
    {
      /// <summary>Called when the attack action is triggered.</summary>
      public abstract void Attack(HeroHsm data, Enemy e);

      /// <summary>Called when an enemy damages the hero.</summary>
      public abstract void TakeDamage(HeroHsm data, int damage);

      /// <summary>Called when the jump action is triggered.</summary>
      public abstract void Jump(HeroHsm data);

      /// <summary>Called when the slide action is triggered.</summary>
      public abstract void Slide(HeroHsm data);

      /// <summary>Called on each tick.</summary>
      public abstract void Process(HeroHsm data, TimeSpan delta);
    }

    private abstract class AliveState : States
    {
      /// <inheritdoc/>
      public sealed override void Attack(HeroHsm data, Enemy e)
      {
        e.TakeDamage(s_attackDamage);
        Console.WriteLine($"{data} Attack: {s_attackDamage}");

        // Apply "reflect" damage.
        // WARNING: uncommenting this leads to reentrancy.
        //data.TakeDamage(5);
      }

      public sealed override void TakeDamage(HeroHsm data, int damage)
      {
        data.m_health -= damage;
        Console.WriteLine($"{data} Damaged: {damage}");
        if (data.m_health <= 0)
        {
          data.m_current = State.Downed;
        }
      }

      public sealed override void Process(HeroHsm data, TimeSpan delta)
      {
        // Move the character...
        data.m_position += (float)delta.TotalSeconds * data.m_velocity;
        data.m_position = Vector2.Clamp(data.m_position, Vector2.Zero, s_screenSize); // Stay on the screen.
        if (data.m_position.Y == 0)
        {
          data.m_current = State.Ground;
        }
        // Apply gravity.
        data.m_velocity += (float)delta.TotalSeconds * data.m_gravity;
        data.m_velocity = Vector2.Clamp(data.m_velocity, s_gravity, s_jumpVelocity);

        // Render the character.
        if (data.m_printRate.TryTake(delta))
        {
          Console.WriteLine($"{data} Position: {data.m_position}");
        }
      }
    }

    private sealed class GroundState : AliveState
    {
      /// <inheritdoc/>
      public override void Jump(HeroHsm data)
      {
        // Jump upwards.
        data.m_velocity += s_jumpVelocity;
        data.m_gravity = s_gravity;
        data.m_current = State.Airborne;
      }

      /// <inheritdoc/>
      public override void Slide(HeroHsm data)
      {
        // Slide sideways.
        data.m_position += s_slideDisplacement;
      }
    }

    private class AirborneState : AliveState
    {
      /// <inheritdoc/>
      public override void Jump(HeroHsm data)
      {
        // Jump while airborne leads to flying.
        data.m_velocity *= Vector2.UnitX;
        data.m_gravity = Vector2.Zero;
        data.m_current = State.Flying;
      }

      /// <inheritdoc/>
      public sealed override void Slide(HeroHsm data)
      {
        // Ignore.  Can't slide while airborne.
      }
    }

    private sealed class FlyingState : AirborneState
    {
      /// <inheritdoc/>
      public override void Jump(HeroHsm data)
      {
        // Jump while flying leads to falling back down.
        data.m_gravity = s_gravity;
        data.m_current = State.Airborne;
      }
    }

    private sealed class DownedState : States
    {
      /// <inheritdoc/>
      public override void Attack(HeroHsm data, Enemy e)
      {
        // Do nothing.
      }

      /// <inheritdoc/>
      public override void TakeDamage(HeroHsm data, int damage)
      {
        // Do nothing.
      }

      /// <inheritdoc/>
      public override void Jump(HeroHsm data)
      {
        // Do nothing.
      }

      /// <inheritdoc/>
      public override void Slide(HeroHsm data)
      {
        // Do nothing.
      }

      /// <inheritdoc/>
      public override void Process(HeroHsm data, TimeSpan delta)
      {
        if (data.m_printRate.TryTake(delta))
        {
          Console.WriteLine("DOWNED");
        }
      }
    }

    ////////////////////////////////////////////////////////////////////////
    // Define State Variables
    ////////////////////////////////////////////////////////////////////////

    /// <summary>The current state of the machine.</summary>
    private States m_current = State.Ground;

    /// <summary>The number of hit points remaining.</summary>
    private int m_health = 20;

    /// <summary>The player's current position.</summary>
    private Vector2 m_position = Vector2.Zero;

    /// <summary>The upward velocity.</summary>
    /// <remarks>This value is negative when travelling down.</remarks>
    private Vector2 m_velocity = Vector2.Zero;

    /// <summary>The velocity due gravity.</summary>
    private Vector2 m_gravity = s_gravity;

    /// <summary>A rate limiter for how often to print (during Process).</summary>
    private readonly BucketRateLimiter m_printRate = new(1, TimeSpan.FromSeconds(1), 1);

    ////////////////////////////////////////////////////////////////////////
    // Define Inputs
    ////////////////////////////////////////////////////////////////////////

    /// <summary>Called when the attack action is triggered.</summary>
    public void Attack(Enemy e)
    {
      States self = m_current;
      m_current.Attack(this, e);
      Contract.Assert(m_current == self);
    }

    /// <summary>Called when attacked by an enemy.</summary>
    public void TakeDamage(int damage)
    {
      States self = m_current;
      m_current.TakeDamage(this, damage);
      Contract.Assert(m_current == self || m_current == State.Downed);
    }

    /// <summary>Called when the jump action is triggered.</summary>
    public void Jump()
    {
      States self = m_current;
      m_current.Jump(this);
      Contract.Assert((self == State.Ground && m_current == State.Airborne) ||
                      (self == State.Airborne && m_current == State.Flying) ||
                      (self == State.Flying && m_current == State.Airborne));
    }

    /// <summary>Called when the slide action is triggered.</summary>
    public void Slide()
    {
      States self = m_current;
      m_current.Slide(this);
      Contract.Assert(m_current == self || m_current == State.Downed);
    }

    /// <summary>Called on each tick.</summary>
    public void Process(TimeSpan delta)
    {
      States self = m_current;
      m_current.Process(this, delta);
      Contract.Assert(m_current == self || m_current == State.Ground);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"{m_current.GetType().Name}";
    }
  }
}
