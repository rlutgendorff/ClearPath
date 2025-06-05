using ClearPath.Reasons;

namespace ClearPath.Results;

public partial class Result
{
    public static Result Ok() => new();

    public static Result<TResult> Ok<TResult>(TResult value) => new(value);

    public static Result Fail(IError error) => new([error]);
    public static Result Fail(IEnumerable<IError> errors) => new(errors);
}

public partial class Result<TValue>
{
    public static Result<TValue> Fail(IError error)
    {
        var result = new Result<TValue>();
        result.Errors.Add(error);
        return result;
    }
    
    public static Result<TValue> Fail(IEnumerable<IError> errors)
    {
        var result = new Result<TValue>();
        result.Errors.AddRange(errors);
        return result;
    }
}