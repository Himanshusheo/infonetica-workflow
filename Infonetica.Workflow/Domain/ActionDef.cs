namespace Infonetica.Workflow.Domain;

public sealed record ActionDef(
    string Id,
    string Name,
    IReadOnlyCollection<string> FromStates,
    string ToState,
    bool Enabled = true
); 