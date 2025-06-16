using ClearPath.Reasons;

namespace ClearPath.AsyncExecutor;

public class AsyncExecutorEvents
{
    public Action<string>? OnStepStart { get; set; }
    public Action<string>? OnStepSuccess { get; set; }
    public Action<string, IReadOnlyList<IError>>? OnStepFailure { get; set; }
    
    public Action<string, Exception>? OnException { get; set; }
    
    public Action<string>? OnCompensationStart { get; set; }
    public Action<string>? OnCompensationSuccess { get; set; }
    public Action<string, IReadOnlyList<IError>>? OnCompensationFailure { get; set; }
}