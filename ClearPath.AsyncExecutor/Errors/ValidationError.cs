using ClearPath.Reasons;

namespace ClearPath.AsyncExecutor.Errors;

public class ValidationError : IError
{
    public string Message { get; set; }
    public Dictionary<string, object> Metadata { get; } = [];
    public List<IError> Reasons { get; init; } = [];
}