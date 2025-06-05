using ClearPath.Reasons;

namespace ClearPath.Results;

public partial class Result
{
    public static implicit operator Result(Error error)
    {
        return Fail(error);
    }
    
    public static implicit operator Result(List<Error> errors)
    {
        return Fail(errors);
    }
}

public partial class Result<TValue>
{
    public static implicit operator Result<TValue>(Result result)
    {
        return result.ToResult<TValue>();
    }
    
    public static implicit operator Result<TValue>(Error error)
    {
        return Fail(error);
    }
    
    public static implicit operator Result<TValue>(TValue value)
    {
        if (value is Result<TValue> r)
            return r;

        return Result.Ok(value);
    }
}