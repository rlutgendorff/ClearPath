using ClearPath.Reasons;

namespace ClearPath.Builders;

public class ResultBuilderEvents
{
    public Action<string>? OnStepStart { get; set; }
    public Action<string>? OnStepSuccess { get; set; }
    public Action<string, List<IError>>? OnStepFailure { get; set; }
}