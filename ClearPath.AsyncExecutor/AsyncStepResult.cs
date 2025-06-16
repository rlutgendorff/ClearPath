using ClearPath.Reasons;

namespace ClearPath.AsyncExecutor;

public class AsyncStepResult
{
    public string Key { get; set; } = "";
    public string? MethodName { get; set; }
    public IReadOnlyList<IError>? Errors { get; set; }
    public bool IsSuccess => Errors == null || Errors.Count == 0;
}