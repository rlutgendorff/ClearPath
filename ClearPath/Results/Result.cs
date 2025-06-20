using IReason = ClearPath.Reasons.IReason;

namespace ClearPath.Results;

public partial class Result : BaseResult
{
  internal Result()
  {
  }

  internal Result(IEnumerable<IReason> reasons)
  {
    Reasons.AddRange(reasons); 
  }
  
  public Result ToResult()
  {
    var result = new Result();
    result.Reasons.AddRange(Reasons);
    return result;  
  }
  
  public Result<TOut> ToResult<TOut>()
  {
    var result = new Result<TOut>();
    result.Reasons.AddRange(Reasons);
    return result;
  }

  public Result<TOut> ToResult<TOut>(TOut value)
  {
    var result = new Result<TOut>();
    result.Reasons.AddRange(Reasons);
    result.Value = value;
    return result;
  }
}

public partial class Result<TValue> : Result
{
  internal Result()
  {
  }

  internal Result(Result result)
  {
    Reasons = result.Reasons;
  }
  
  internal Result(TValue value)
  {
    Value = value;
  }

  public TValue? Value { get; set; }
  public TValue ValueOrDefault => IsSuccess ? Value! : default!;

    public Result<TOut> OnSuccess<TOut>(Func<TValue, Result<TOut>> func)
  {
    return IsSuccess ? func(Value) : ToResult<TOut>();
  }

  public Result OnSuccess(Func<TValue, Result> func)
  {
    return IsSuccess ? func(Value) : ToResult();
  }

  public Result OnSuccess(Func<Result> func)
  {
    return IsSuccess ? func() : ToResult();
  }

  public Task<Result<TOut>> OnSuccess<TOut>(Func<TValue, Task<Result<TOut>>> func)
  {
    return IsSuccess ? func(Value) : Task.FromResult(ToResult<TOut>());
  }

  public Result<(TValue, TNew)> Combine<TNew>(Result<TNew> toCombine)
  {
    return (Value, toCombine.Value);
  }

  private Result ToResult()
  {
    var result = new Result();
    result.Reasons.AddRange(Reasons);
    return result;  
  }
  
  private Result<TOut> ToResult<TOut>()
  {
    var result = new Result<TOut>();
    result.Reasons.AddRange(Reasons);
    return result;
  }
}