namespace Infonetica.Workflow.Dtos;

public sealed record ApiError(string Code, string Message, object? Details = null); 