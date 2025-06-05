using ClearPath.Reasons;
using System.Collections.Generic;

namespace ClearPath.Results;

public class StepResultMetadata
{
    public string? FirstFailure { get; set; }
    public int Steps { get; set; }
    public Dictionary<string, object> AdditionalData { get; } = new();
}

public class StepResult : Result
{
    public StepResultMetadata Metadata { get; } = new();

    internal StepResult()
    {
    }

    internal StepResult(IEnumerable<IReason> reasons) : base(reasons)
    {
    }

    internal StepResult(Result result) : base(result.Reasons)
    {
    }

    public static StepResult Ok() => new();

    public static StepResult<TValue> Ok<TValue>(TValue value) => new(value);

    public static StepResult Fail(IError error) => new([error]);

    public static StepResult Fail(IEnumerable<IError> errors) => new(errors);
}

public class StepResult<TValue> : Result<TValue>
{
    public StepResultMetadata Metadata { get; } = new();

    internal StepResult()
    {
    }

    internal StepResult(Result result) : base(result)
    {
    }

    internal StepResult(TValue value) : base(value)
    {
    }

    public static StepResult<TValue> Ok(TValue value) => new(value);

    public static StepResult<TValue> Fail(IError error)
    {
        var result = new StepResult<TValue>();
        result.Reasons.Add(error);
        return result;
    }

    public static StepResult<TValue> Fail(IEnumerable<IError> errors)
    {
        var result = new StepResult<TValue>();
        result.Reasons.AddRange(errors);
        return result;
    } 
}
