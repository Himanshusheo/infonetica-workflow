using Infonetica.Workflow.Domain;

namespace Infonetica.Workflow.Dtos;

public sealed class CreateDefinitionRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<StateDef>? States { get; set; }
    public List<CreateActionDto>? Actions { get; set; }
}

public sealed class CreateActionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
} 