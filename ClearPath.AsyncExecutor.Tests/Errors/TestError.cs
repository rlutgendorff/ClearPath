using ClearPath.Reasons;

namespace ClearPath.AsyncExecutor.Tests.Errors;

public class TestError : IError
{
    public string Message { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public List<IError> Reasons { get; set; }
}