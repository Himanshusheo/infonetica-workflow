using System.Collections.Concurrent;
using Infonetica.Workflow.Domain;

namespace Infonetica.Workflow.Services;

public sealed class WorkflowService
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, WorkflowInstance> _instances = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<WorkflowDefinition> ListDefinitions() => _definitions.Values.ToList();
    public IReadOnlyCollection<WorkflowInstance> ListInstances() => _instances.Values.ToList();

    public bool TryGetDefinition(string id, out WorkflowDefinition def) => _definitions.TryGetValue(id, out def!);
    public bool TryGetInstance(string id, out WorkflowInstance inst) => _instances.TryGetValue(id, out inst!);

    public (WorkflowDefinition? Def, string? Error) CreateDefinition(
        string? id,
        string name,
        IEnumerable<StateDef> states,
        IEnumerable<ActionDef> actions)
    {
        var defId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("n") : id.Trim();

        // basic validation
        var stateList = states?.ToList() ?? new();
        if (stateList.Count == 0) return (null, "Definition must contain at least one state.");
        if (stateList.Count(s => s.IsInitial) != 1) return (null, "Definition must contain exactly one initial state.");
        var dupState = stateList.GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (dupState != null) return (null, $"Duplicate state id: {dupState.Key}");

        var actionList = actions?.ToList() ?? new();
        var dupAction = actionList.GroupBy(a => a.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (dupAction != null) return (null, $"Duplicate action id: {dupAction.Key}");

        var stateIds = new HashSet<string>(stateList.Select(s => s.Id), StringComparer.OrdinalIgnoreCase);
        foreach (var a in actionList)
        {
            if (!stateIds.Contains(a.ToState)) return (null, $"Action '{a.Id}' references unknown ToState '{a.ToState}'.");
            foreach (var fs in a.FromStates)
                if (!stateIds.Contains(fs)) return (null, $"Action '{a.Id}' references unknown FromState '{fs}'.");
        }

        var def = new WorkflowDefinition(defId, name, stateList, actionList);
        if (!_definitions.TryAdd(defId, def)) return (null, $"Definition id '{defId}' already exists.");
        return (def, null);
    } 

    public (WorkflowInstance? Inst, string? Error) StartInstance(string defId, string? id = null)
    {
        if (!TryGetDefinition(defId, out var def)) return (null, $"Definition '{defId}' not found.");
        var instId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("n") : id.Trim();
        if (_instances.ContainsKey(instId)) return (null, $"Instance id '{instId}' already exists.");
        var inst = new WorkflowInstance(instId, defId, def.InitialStateId);
        _instances[instId] = inst;
        return (inst, null);
    }

    public (WorkflowInstance? Inst, string? Error) ExecuteAction(string instId, string actionId)
    {
        if (!TryGetInstance(instId, out var inst)) return (null, $"Instance '{instId}' not found.");
        if (!TryGetDefinition(inst.DefinitionId, out var def)) return (null, $"Definition '{inst.DefinitionId}' not found.");
        if (!def.Actions.TryGetValue(actionId, out var action)) return (null, $"Action '{actionId}' not found in definition '{def.Id}'.");
        if (!action.Enabled) return (null, $"Action '{actionId}' is disabled.");
        if (!def.States.TryGetValue(inst.CurrentStateId, out var curState)) return (null, $"Current state '{inst.CurrentStateId}' missing in definition.");
        if (!curState.Enabled) return (null, $"Current state '{curState.Id}' is disabled.");
        if (curState.IsFinal) return (null, $"Instance is in final state '{curState.Id}'. No further actions allowed.");
        if (!action.FromStates.Contains(curState.Id, StringComparer.OrdinalIgnoreCase)) return (null, $"Action '{actionId}' cannot be executed from state '{curState.Id}'.");
        if (!def.States.TryGetValue(action.ToState, out var toState)) return (null, $"Target state '{action.ToState}' missing in definition.");
        if (!toState.Enabled) return (null, $"Target state '{toState.Id}' is disabled.");

        var hist = new InstanceHistoryEntry(DateTimeOffset.UtcNow, action.Id, curState.Id, toState.Id);
        inst.History.Add(hist);
        inst.CurrentStateId = toState.Id;
        return (inst, null);
    }
} 