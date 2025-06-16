using System.Runtime.CompilerServices;
using ClearPath.Results;
using FluentValidation;
using Polly;

namespace ClearPath.AsyncExecutor;

public static class AsyncExecutorExtensions
{
    public static async Task<AsyncExecutor> AddKeyedVariable<T>(this Task<AsyncExecutor> task, string key, T value)
    {
        var executor = await task;
        return executor.AddKeyedVariable(key, value);
    }

    public static async Task<AsyncExecutor> AddKeyedVariable<T>(this Task<AsyncExecutor> task, string key,
        IResult<T> result)
    {
        var executor = await task;
        return executor.AddKeyedVariable(key, result);
    }


    public static async Task<AsyncExecutor> AddKeyedVariable<T>(this Task<AsyncExecutor> task, string key,
        Task<IResult<T>> resultTask)
    {
        var executor = await task;
        return executor.AddKeyedVariable(key, resultTask);
    }

    public static async Task<AsyncExecutor> Then<TOut>(this Task<AsyncExecutor> task, string key, Func<CancellationToken, Task<IResult<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> Then<T1, TOut>(this Task<AsyncExecutor> task, string key, Func<T1, CancellationToken, Task<IResult<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> Then<T1, T2, TOut>(this Task<AsyncExecutor> task, string key, Func<T1, T2, CancellationToken, Task<IResult<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> Then<T1, T2, T3, TOut>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, T3, CancellationToken, Task<IResult<TOut>>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc, policy, cancellationToken);
    }
    
    public static async Task<AsyncExecutor> ThenWithCompensation<TOut> (
        this Task<AsyncExecutor> task,
        string key,
        Func<CancellationToken, Task<IResult<TOut>>> stepFunc,
        string compensationKey,
        Func<AsyncExecutorContext, Task<IResult>> compensationFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenWithCompensation(key, stepFunc, compensationKey, compensationFunc, policy, cancellationToken);
    }
    public static async Task<AsyncExecutor> ThenWithCompensation<T1, TOut>(
        this Task<AsyncExecutor> task, string key,
        Func<T1, CancellationToken, Task<IResult<TOut>>> stepFunc,
        string compensationKey,
        Func<AsyncExecutorContext, Task<IResult>> compensationFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenWithCompensation(key, stepFunc, compensationKey, compensationFunc, policy,
            cancellationToken);
    }
    
    public static async Task<AsyncExecutor> ThenWithCompensation<T1, T2, TOut>(
        this Task<AsyncExecutor> task, string key,
        Func<T1, T2, CancellationToken, Task<IResult<TOut>>> stepFunc,
        string compensationKey,
        Func<AsyncExecutorContext, Task<IResult>> compensationFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenWithCompensation(key, stepFunc, compensationKey, compensationFunc, policy,
            cancellationToken);
    }
    
    public static async Task<AsyncExecutor> ThenWithCompensation<T1, T2, T3, TOut>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, T3, CancellationToken, Task<IResult<TOut>>> stepFunc, 
        string compensationKey,
        Func<AsyncExecutorContext, Task<IResult>> compensationFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenWithCompensation(key, stepFunc, compensationKey, compensationFunc, policy,
            cancellationToken);
    }
    
    public static async Task<AsyncExecutor> ThenIf<TOut>(
        this Task<AsyncExecutor> task, string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<CancellationToken, Task<IResult<TOut>>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenIf(key, ifPredicate, skipIfPredicate, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> ThenIf<T1, TOut>(
        this Task<AsyncExecutor> task, string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<T1, CancellationToken, Task<IResult<TOut>>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenIf(key, ifPredicate, skipIfPredicate, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> ThenIf<T1, T2, TOut>(
        this Task<AsyncExecutor> task, string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<T1, T2, CancellationToken, Task<IResult<TOut>>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenIf(key, ifPredicate, skipIfPredicate, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> ThenIf<T1, T2, T3, TOut>(
        this Task<AsyncExecutor> task, string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<T1, T2, T3, CancellationToken, Task<IResult<TOut>>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.ThenIf(key, ifPredicate, skipIfPredicate, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> Do(
        this Task<AsyncExecutor> task, string key,
        Func<CancellationToken, Task<IResult>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc, policy, cancellationToken);
    }
    
    public static async Task<AsyncExecutor> Do<T1>(
        this Task<AsyncExecutor> task,
        string key, 
        Func<T1, CancellationToken, Task<IResult>> stepFunc,
        IAsyncPolicy? policy = null, 
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc, policy, cancellationToken);
    }
    
    public static async Task<AsyncExecutor> Do<T1, T2>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, CancellationToken, Task<IResult>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> Do<T1, T2, T3>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, T3, CancellationToken, Task<IResult>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc, policy, cancellationToken);
    }

    public static async Task<AsyncExecutor> FinishAll(this Task<AsyncExecutor> task)
    {
        var executor = await task;
        return await executor.FinishAll();
    }
    
    public static async Task<IResult> GetResult(this Task<AsyncExecutor> task)
    {
        var executor = await task;
        return await executor.GetResult();
    }
    
    public static async Task<IResult<T>> GetResult<T>(this Task<AsyncExecutor> task, string key)
    {
        var executor = await task;
        return await executor.GetResult<T>(key);
    }

    public static async Task<AsyncExecutor> GetEvents(this Task<AsyncExecutor> task, AsyncExecutorEvents events)
    {
        var executor = await task;
        return executor.WithEvents(events);
    }

    public static async Task<AsyncExecutor> WithPolicy(this Task<AsyncExecutor> task, IAsyncPolicy policy)
    {
        var executor = await task;
        return executor.WithPolicy(policy);
    }
    
    public static async Task<AsyncExecutor> WithValidator<T>(this Task<AsyncExecutor> task, IValidator<T> validator)
    {
        var executor = await task;
        return executor.WithValidator(validator);
    }

    public static async Task<AsyncExecutor> CompensateAll(this Task<AsyncExecutor> task)
    {
        var executor = await task;
        await executor.CompensateAll();
        return executor;
    }
}