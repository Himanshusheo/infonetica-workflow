using System.Text.Json;
using Infonetica.Workflow.Domain;
using Infonetica.Workflow.Services;

namespace Infonetica.Workflow.Persistence;

public static class JsonSnapshotStore
{
    private sealed record Snapshot(
        List<WorkflowDefinition> Definitions,
        List<WorkflowInstance> Instances
    );

    public static string ExportJson(WorkflowService svc)
    {
        var snap = new Snapshot(svc.ListDefinitions().ToList(), svc.ListInstances().ToList());
        return JsonSerializer.Serialize(snap, new JsonSerializerOptions { WriteIndented = true });
    }

    public static (bool Ok, string? Error) ImportJson(string json, WorkflowService svc)
    {
        Snapshot? snap;
        try
        {
            snap = JsonSerializer.Deserialize<Snapshot>(json);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
        if (snap == null) return (false, "Invalid snapshot.");

        // naive: rebuild service via reflection (service fields are private). Instead, re-add through API ops.
        // clear existing by creating new service? not possible w/out DI rewire. We'll do simple: process definitions then instances.
        foreach (var d in snap.Definitions)
        {
            svc.CreateDefinition(d.Id, d.Name, d.States.Values, d.Actions.Values);
        }
        foreach (var i in snap.Instances)
        {
            // try to recreate; if id exists skip
            if (!svc.TryGetDefinition(i.DefinitionId, out var def)) continue;
            var (inst, _) = svc.StartInstance(def.Id, i.Id);
            if (inst == null) continue;
            inst.CurrentStateId = i.CurrentStateId;
            inst.History.Clear();
            inst.History.AddRange(i.History);
        }
        return (true, null);
    }
} 