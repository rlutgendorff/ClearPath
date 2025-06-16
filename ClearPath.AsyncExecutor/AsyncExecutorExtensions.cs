using ClearPath.Results;

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

    public static async Task<AsyncExecutor> Then<TOut>(this Task<AsyncExecutor> task, string key, Func<Task<IResult<TOut>>> stepFunc)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc);
    }

    public static async Task<AsyncExecutor> Then<T1, TOut>(this Task<AsyncExecutor> task, string key, Func<T1, Task<IResult<TOut>>> stepFunc)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc);
    }

    public static async Task<AsyncExecutor> Then<T1, T2, TOut>(this Task<AsyncExecutor> task, string key, Func<T1, T2, Task<IResult<TOut>>> stepFunc)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc);
    }

    public static async Task<AsyncExecutor> Then<T1, T2, T3, TOut>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, T3, Task<IResult<TOut>>> stepFunc)
    {
        var executor = await task;
        return await executor.Then(key, stepFunc);
    }

    public static async Task<AsyncExecutor> Do(this Task<AsyncExecutor> task, string key, Func<Task<IResult>> stepFunc)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc);
    }
    
    public static async Task<AsyncExecutor> Do<T1>(this Task<AsyncExecutor> task, string key, Func<T1, Task<IResult>> stepFunc)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc);
    }
    
    public static async Task<AsyncExecutor> Do<T1, T2>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, Task<IResult>> stepFunc)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc);
    }

    public static async Task<AsyncExecutor> Do<T1, T2, T3>(
        this Task<AsyncExecutor> task,
        string key,
        Func<T1, T2, T3, Task<IResult>> stepFunc)
    {
        var executor = await task;
        return await executor.Do(key, stepFunc);
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
}