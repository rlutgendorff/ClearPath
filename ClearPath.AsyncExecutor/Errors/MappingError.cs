using ClearPath.Reasons;

namespace ClearPath.AsyncExecutor.Errors;

public class MappingError : IError
{
    public string Message { get; set; }
    public Dictionary<string, object> Metadata { get; } = [];
    public List<IError> Reasons { get; } = [];
}