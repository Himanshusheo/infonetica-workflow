using Infonetica.Workflow.Domain;

namespace Infonetica.Workflow.Dtos;

public sealed record DefinitionDto(
    string Id,
    string Name,
    IEnumerable<StateDef> States,
    IEnumerable<ActionDef> Actions,
    string InitialStateId
); 