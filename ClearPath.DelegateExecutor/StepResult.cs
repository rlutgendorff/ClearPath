using ClearPath.Reasons;
using ClearPath.Results;

namespace ClearPath.DelegateExecutor;

public class StepResult
{
    public string ReturnKey { get; set; } = string.Empty;
    public Task<object> ResolvedTask { get; set; } = Task.FromResult<object>(null!);
    public List<string> ParameterKeys { get; set; } = new();
    public string? MethodName { get; set; }
    public IReadOnlyList<IError>? Errors { get; set; }
    public bool IsSuccess => Errors == null || Errors.Count == 0;
}