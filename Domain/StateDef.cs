namespace Infonetica.Workflow.Domain;

public sealed record StateDef(
    string Id,
    string Name,
    bool IsInitial = false,
    bool IsFinal = false,
    bool Enabled = true
); 