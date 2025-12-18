using MarymoorStudios.Core;
using MarymoorStudios.Core.Fsm;

namespace HsmSample;

[Hsm]
internal sealed partial class Knife : Character
{
  /// <summary>The fire rate.</summary>
  public BucketRateLimiter FireRate { get; set; }

  /// <summary>
  /// The destination for a throw.
  /// </summary>
  private float m_destination;

  /// <summary>The state machine.</summary>
  private readonly Hsm m_state;

  public Knife()
  {
    Token = '-';
    Position = 45;
    Velocity = 0;
    SightDistance = 1;
    Damage = 10;

    MovementSpeed = 10;
    FireRate = new BucketRateLimiter(1, TimeSpan.FromSeconds(1), 1);
    m_destination = 0;

    m_state = Hsm.Create(this, StateMachine);
  }

  [HsmInput]
  public override void Process(double delta)
  {
    m_state.Process(this, delta);
  }

  [HsmInput]
  public void Throw(float maxDistance)
  {
    m_state.Throw(this, maxDistance);
  }

  [HsmInput]
  public void Pickup()
  {
    m_state.Pickup(this);
  }

  /// <inheritdoc/>
  public override void TakeDamage(int damage)
  {
    // Ignore.
  }

  [HsmStates]
  private static void StateMachine(HsmMeta<Knife, Hsm, State, Inputs> m, Inputs i, Knife x)
  {
    m.Composite(State.Init,
      [
        m.Composite(State.Active,
        [
          m.State(State.Thrown)
           .Edge(i.Process, x.ThrownProcess, State.Self, State.Idle),
          m.State(State.Held)
           .Edge(i.Process, x.HeldProcess, State.Self, State.Idle)
           .Edge(i.Throw, x.HeldThrow, State.Thrown),
        ]),
        m.State(State.Idle)
         .OnEnter(x.IdleEnter)
         .OnExit(x.IdleExit)
         .Edge(i.Pickup, x.IdlePickup, State.Held),
      ])
     .OnStart(State.Idle);
  }

  private void IdleEnter(Hsm m)
  {
    Token = '_';
  }

  private void IdleExit(Hsm m)
  {
    Token = '-';
  }

  private void IdlePickup(Hsm m)
  {
    m.SetNext(State.Held);
  }

  private void ThrownProcess(Hsm m, double delta)
  {
    Position += (float)(delta * Velocity); // Move.
    // Don't move out of board.
    Position = MathF.Min(MathF.Max(Position, 0), 100);
    if (Position is 0 or 100)
    {
      m.SetNext(State.Idle);  // Fall down at the boarders.
      return;
    }
    if (Facing < 0 && Position <= m_destination || Facing > 0 && Position >= m_destination)
    {
      m.SetNext(State.Idle);  // Fall down at the destination.
      return;
    }

    // Hit players (and then fall on the ground).
    if (SceneTree?.RayTrace<Player>(Position, Facing, 1, out Player? p) ?? false)
    {
      p.TakeDamage(Damage * 3);
      m.SetNext(State.Idle); // Fall on the ground.
    }
  }

  private void HeldProcess(Hsm arg1, double delta)
  {
    // Cooldown.
    FireRate.Process(delta);

    // Slash players.
    if (SceneTree?.RayTrace<Player>(Position, Facing, 1, out Player? p) ?? false)
    {
      if (FireRate.TryTake())
      {
        FireRate.Reset();
        p.TakeDamage(Damage);
      }
    }
  }

  private void HeldThrow(Hsm m, float maxDistance)
  {
    m_destination = Position + MathF.Abs(maxDistance) * Facing;
    m_destination = MathF.Min(MathF.Max(m_destination, 0), 100);
    Velocity = MovementSpeed * Facing;
    m.SetNext(State.Thrown);
  }
}
