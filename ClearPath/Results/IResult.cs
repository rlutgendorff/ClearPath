using ClearPath.Reasons;

namespace ClearPath.Results;

public interface IResult
{
    bool IsFailed { get; }
    
    bool IsSuccess { get; }
    
    List<IReason> Reasons { get; }
    
    List<IError> Errors { get; }
    
    List<ISuccess> Successes { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue Value { get; }
    
    TValue ValueOrDefault { get; }
}