namespace Infonetica.Workflow.Domain;

public sealed class WorkflowInstance
{
    public string Id { get; init; }
    public string DefinitionId { get; init; }
    public string CurrentStateId { get; internal set; }
    public List<InstanceHistoryEntry> History { get; } = new();

    public WorkflowInstance(string id, string definitionId, string startStateId)
    {
        Id = id;
        DefinitionId = definitionId;
        CurrentStateId = startStateId;
    }
} 