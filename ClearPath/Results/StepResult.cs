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
}
