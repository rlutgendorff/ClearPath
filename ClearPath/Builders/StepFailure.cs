using ClearPath.Reasons;

namespace ClearPath.Builders;

public class StepFailure
{
    public string Key { get; }
    public IReadOnlyList<IError> Errors { get; }

    public StepFailure(string key, IEnumerable<IError> errors)
    {
        Key = key;
        Errors = errors.ToList();
    }
}