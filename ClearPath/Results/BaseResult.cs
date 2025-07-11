﻿using ClearPath.Reasons;

namespace ClearPath.Results;

public abstract class BaseResult : IResult
{
    public bool IsFailed => Errors.Count != 0;
    public bool IsSuccess => !IsFailed;
    public List<IReason> Reasons { get; protected set; } = [];
    public IReadOnlyList<IError> Errors => Reasons.OfType<IError>().ToList();
    public IReadOnlyList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToList();
}

public abstract class BaseResult<TResult> : BaseResult, IResult<TResult>
{
    public TResult? Value { get; set; }
    public TResult ValueOrDefault => IsSuccess ? Value! : default!;
}
