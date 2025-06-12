using ClearPath.Results;

namespace ClearPath.Executors;

public static class FluentExecutorExtensions
{
    public static Task<FluentExecutor> Then<T>(this Task<FluentExecutor> task, Func<Task<IResult<T>>> func)
    {
        var executor = task.Result;
        return executor.Then(func);
    }
    
    public static Task<FluentExecutor> Then<T1, T>(this Task<FluentExecutor> task, Func<T1, Task<IResult<T>>> func)
    {
        var executor = task.Result;
        return executor.Then(func);
    }
    
    public static Task<FluentExecutor> Then<T1, T2, T>(this Task<FluentExecutor> task, Func<T1, T2, Task<IResult<T>>> func)
    {
        var executor = task.Result;
        return executor.Then(func);
    }
    
    public static Task<FluentExecutor> Then<T1, T2, T3, T>(this Task<FluentExecutor> task, Func<T1, T2, T3, Task<IResult<T>>> func)
    {
        var executor = task.Result;
        return executor.Then(func);
    }

    public static Task<FluentExecutor> Then(this Task<FluentExecutor> task, Func<Task<IResult>> func, string key)
    {
        var executor = task.Result;
        return executor.Then(func, key);
    }

    public static Task<FluentExecutor> Then<T1>(this Task<FluentExecutor> task, Func<T1, Task<IResult>> func, string key)
    {
        var executor = task.Result;
        return executor.Then(func, key);
    }

    public static Task<FluentExecutor> Then<T1, T2>(this Task<FluentExecutor> task, Func<T1, T2, Task<IResult>> func, string key)
    {
        var executor = task.Result;
        return executor.Then(func, key);
    }

    public static Task<FluentExecutor> Then<T1, T2, T3>(this Task<FluentExecutor> task, Func<T1, T2, T3, Task<IResult>> func, string key)
    {
        var executor = task.Result;
        return executor.Then(func, key);
    }
}