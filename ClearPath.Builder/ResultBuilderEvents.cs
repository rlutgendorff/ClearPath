using ClearPath.Reasons;

namespace ClearPath.Builder;

public class ResultBuilderEvents
{
    public Action<string>? OnStepStart { get; set; }
    public Action<string>? OnStepSuccess { get; set; }
    public Action<string, IReadOnlyList<IError>>? OnStepFailure { get; set; }
}