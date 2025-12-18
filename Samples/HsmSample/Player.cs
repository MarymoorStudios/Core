using MarymoorStudios.Core;
using MarymoorStudios.Core.Fsm;

namespace HsmSample;

[Hsm]
internal sealed partial class Player : Character
{
  /// <summary>The amount of time the character will remain stunned.</summary>
  public BucketRateLimiter StunTime { get; set; }

  /// <summary>The state machine.</summary>
  private readonly Hsm m_state;

  public Player()
  {
    Token = '*';
    Position = 20;
    Velocity = 0;
    SightDistance = 20;
    Health = 30;
    Damage = 5;
    MovementSpeed = 5;

    StunTime = new BucketRateLimiter(1, TimeSpan.FromSeconds(1.5));

    m_state = Hsm.Create(this, StateMachine);
  }

  [HsmInput]
  public override void Process(double delta)
  {
    m_state.Process(this, delta);
  }

  [HsmInput]
  public override void TakeDamage(int damage)
  {
    m_state.TakeDamage(this, damage);
  }

  /// <inheritdoc/>
  public override InputOutcome HandleInput(ConsoleKeyInfo key)
  {
    switch (key.Key)
    {
      case ConsoleKey.LeftArrow:
      case ConsoleKey.RightArrow:
      case ConsoleKey.Spacebar:
        OnInput(key.Key);
        return InputOutcome.Handled;
      case ConsoleKey.Q:
        return InputOutcome.Terminate;
      case var _:
        return InputOutcome.None;
    }
  }

  [HsmInput]
  public void OnInput(ConsoleKey key)
  {
    m_state.OnInput(this, key);
  }

  [HsmStates]
  private static void StateMachine(HsmMeta<Player, Hsm, State, Inputs> m, Inputs i, Player x)
  {
    m.Composite(State.Init,
      [
        m.Composite(State.Alive,
          [
            m.State(State.Stunned)
             .OnEnter(x.StunnedOnEnter)
             .OnExit(x.StunnedOnExit)
             .Edge(i.Process, x.StunnedProcess, State.Self, State.History),
            m.State(State.Normal)
             .Edge(i.Process, x.NormalProcess)
             .Edge(i.OnInput, x.NormalOnInput),
          ])
         .OnStart(State.Normal)
         .Edge(i.TakeDamage, x.AliveTakeDamage, State.Dead, State.Stunned),
        m.State(State.Dead)
         .OnEnter(x.DeadOnEnter),
      ])
     .OnStart(State.Alive);
  }

  private void AliveTakeDamage(Hsm m, int damage)
  {
    Health = Math.Max(0, Health - damage);
    if (Health <= 0)
    {
      Token = '&';
      m.SetNext(State.Dead);
      return;
    }
    m.SetNext(State.Stunned);
  }

  private void StunnedOnEnter(Hsm m)
  {
    StunTime.Reset();
    Token = '@';
  }

  private void StunnedOnExit(Hsm m)
  {
    Token = '*';
  }

  private void StunnedProcess(Hsm m, double delta)
  {
    if (StunTime.TryTake(delta))
    {
      m.SetNext(State.History);
      return;
    }

    m.SetNext(State.Stunned);
  }

  private void NormalProcess(Hsm m, double delta)
  {
    // Move.
    Position += (float)(delta * Velocity);
    // Don't move out of board.
    Position = MathF.Min(MathF.Max(Position, 0), 100);
  }

  private void NormalOnInput(Hsm m, ConsoleKey key)
  {
    switch (key)
    {
      case ConsoleKey.LeftArrow:
        Velocity = Math.Max(-MovementSpeed, Velocity - MovementSpeed);
        break;
      case ConsoleKey.RightArrow:
        Velocity = Math.Min(MovementSpeed, Velocity + MovementSpeed);
        break;
      case ConsoleKey.Spacebar:
        Attack();
        break;
    }
  }

  private void Attack()
  {
    // Attack the closest Enemy.
    if (SceneTree?.RayTrace<Enemy>(Position, Facing, SightDistance, out Enemy? e) ?? false)
    {
      e.TakeDamage(Damage);
    }
  }

  private void DeadOnEnter(Hsm m)
  {
    SceneTree?.Terminate = true;
  }
}
