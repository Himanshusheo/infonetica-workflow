namespace Infonetica.Workflow.Domain;

public sealed record InstanceHistoryEntry(
    DateTimeOffset Timestamp,
    string ActionId,
    string FromStateId,
    string ToStateId
); 