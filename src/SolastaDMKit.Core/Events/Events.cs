namespace SolastaDMKit.Core.Events;

public sealed class TileEntered : ISxEvent
{
    public GameLocationCharacter Character { get; internal set; }
    public object Source { get; internal set; }
    public object Destination { get; internal set; }
}

public sealed class ObjectInteracted : ISxEvent
{
    public GameGadget Gadget { get; internal set; }
    public int ConditionIndex { get; internal set; }
    public bool NewState { get; internal set; }
    public bool Skip { get; set; }
}

public sealed class DamageAbout : ISxEvent
{
    public RulesetActor Target { get; internal set; }
    public int RolledDamage { get; internal set; }
    public string DamageType { get; internal set; }
    public RulesetImplementationDefinitions.ApplyFormsParams FormsParams { get; internal set; }
    public RollInfo RollInfo { get; internal set; }
    public bool Skip { get; set; }
    public int? OverrideAmount { get; set; }
}

public sealed class DamageApplied : ISxEvent
{
    public RulesetActor Target { get; internal set; }
    public int RolledDamage { get; internal set; }
    public string DamageType { get; internal set; }
    public RulesetImplementationDefinitions.ApplyFormsParams FormsParams { get; internal set; }
}

public sealed class TurnStarted : ISxEvent
{
    public GameLocationCharacter Character { get; internal set; }
}
