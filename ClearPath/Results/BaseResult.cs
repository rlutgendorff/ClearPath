using ClearPath.Reasons;

namespace ClearPath.Results;

public abstract class BaseResult
{
    public bool IsFailed => Errors.Count != 0;
    public bool IsSuccess => !IsFailed;
    public List<IReason> Reasons { get; protected set; } = [];
    public IReadOnlyList<IError> Errors => Reasons.OfType<IError>().ToList();
    public IReadOnlyList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToList();
}
