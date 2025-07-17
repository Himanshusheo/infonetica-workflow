using Infonetica.Workflow.Domain;
using Infonetica.Workflow.Dtos;
using Infonetica.Workflow.Persistence;
using Infonetica.Workflow.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WorkflowService>();
var app = builder.Build();

// ---- Definitions ----
app.MapPost("/workflow-definitions", (CreateDefinitionRequest req, WorkflowService svc) =>
{
    var states = req.States ?? new();
    var actions = (req.Actions ?? new()).Select(a => new ActionDef(a.Id, a.Name, a.FromStates, a.ToState, a.Enabled));
    var (def, err) = svc.CreateDefinition(req.Id, req.Name, states, actions);
    return def is not null
        ? Results.Created($"/workflow-definitions/{def.Id}", ToDto(def))
        : Results.BadRequest(new ApiError("invalid_definition", err ?? "error"));
});

app.MapGet("/workflow-definitions", (WorkflowService svc) =>
{
    var list = svc.ListDefinitions().Select(ToDto);
    return Results.Ok(list);
});

app.MapGet("/workflow-definitions/{id}", (string id, WorkflowService svc) =>
{
    return svc.TryGetDefinition(id, out var def)
        ? Results.Ok(ToDto(def))
        : Results.NotFound(new ApiError("not_found", $"Definition '{id}' not found."));
});

// ---- Instances ----
app.MapPost("/workflow-instances", (StartInstanceRequest req, WorkflowService svc) =>
{
    var (inst, err) = svc.StartInstance(req.DefinitionId, req.Id);
    if (inst is null) return Results.BadRequest(new ApiError("start_failed", err ?? "error"));
    return Results.Created($"/workflow-instances/{inst.Id}", ToInstanceDto(inst, svc));
});

app.MapGet("/workflow-instances", (WorkflowService svc) =>
{
    var list = svc.ListInstances().Select(i => ToInstanceDto(i, svc));
    return Results.Ok(list);
});

app.MapGet("/workflow-instances/{id}", (string id, WorkflowService svc) =>
{
    if (!svc.TryGetInstance(id, out var inst))
        return Results.NotFound(new ApiError("not_found", $"Instance '{id}' not found."));
    return Results.Ok(ToInstanceDto(inst, svc));
});

app.MapPost("/workflow-instances/{id}/actions/{actionId}", (string id, string actionId, WorkflowService svc) =>
{
    var (inst, err) = svc.ExecuteAction(id, actionId);
    if (inst is null) return Results.BadRequest(new ApiError("execute_failed", err ?? "error"));
    return Results.Ok(ToInstanceDto(inst, svc));
});

// ---- Admin / Snapshot ----
app.MapPost("/_admin/export", (WorkflowService svc) =>
{
    var json = JsonSnapshotStore.ExportJson(svc);
    return Results.Text(json, "application/json");
});

app.MapPost("/_admin/import", async (HttpRequest http, WorkflowService svc) =>
{
    using var reader = new StreamReader(http.Body);
    var json = await reader.ReadToEndAsync();
    var (ok, err) = JsonSnapshotStore.ImportJson(json, svc);
    return ok ? Results.Ok() : Results.BadRequest(new ApiError("import_failed", err ?? "error"));
});

app.Run();

// ---- local helpers ----
static DefinitionDto ToDto(WorkflowDefinition d) => new(
    d.Id,
    d.Name,
    d.States.Values,
    d.Actions.Values,
    d.InitialStateId
);

static InstanceDto ToInstanceDto(Infonetica.Workflow.Domain.WorkflowInstance i, WorkflowService svc)
{
    svc.TryGetDefinition(i.DefinitionId, out var def);
    var cur = i.CurrentStateId;
    var isFinal = def != null && def.States.TryGetValue(cur, out var s) && s.IsFinal;
    return new InstanceDto(i.Id, i.DefinitionId, cur, isFinal, i.History);
} 