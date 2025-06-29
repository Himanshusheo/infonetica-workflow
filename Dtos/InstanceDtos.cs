using Infonetica.Workflow.Domain;

namespace Infonetica.Workflow.Dtos;

public sealed record StartInstanceRequest(string DefinitionId, string? Id = null);

public sealed record InstanceDto(
    string Id,
    string DefinitionId,
    string CurrentState,
    bool IsFinal,
    IReadOnlyList<InstanceHistoryEntry> History
); 