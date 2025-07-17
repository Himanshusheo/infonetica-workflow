namespace Infonetica.Workflow.Domain;

public sealed class WorkflowDefinition
{
    public string Id { get; init; }
    public string Name { get; init; }
    public IReadOnlyDictionary<string, StateDef> States { get; init; }
    public IReadOnlyDictionary<string, ActionDef> Actions { get; init; }
    public string InitialStateId { get; init; }

    public WorkflowDefinition(string id, string name,
        IEnumerable<StateDef> states,
        IEnumerable<ActionDef> actions)
    {
        Id = id;
        Name = name;
        var stateDict = states.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);
        States = stateDict;
        var actionDict = actions.ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);
        Actions = actionDict;
        InitialStateId = stateDict.Values.Single(s => s.IsInitial).Id;
    }
} 