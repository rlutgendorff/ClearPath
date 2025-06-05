using ClearPath.Reasons;

namespace ClearPath.Builders;

public class StepFailure
{
    public string Key { get; }
    public List<IError> Errors { get; }

    public StepFailure(string key, List<IError> errors)
    {
        Key = key;
        Errors = errors;
    }
}