using ClearPath.Builders;
using ClearPath.Results;

namespace ClearPath.Extensions;

public static class ResultBuilderExtensions
{
    public static async Task<ResultBuilder> DoWhenSuccessAsync<T>(
        this Task<ResultBuilder> task, 
        string key, 
        Func<ResultBuilder.ResultBuilderContext, Task<IResult<T>>> func)
    {
        var builder = await task;
        return await builder.DoWhenSuccessAsync(key, func);
    }
    
    public static async Task<ResultBuilder> DoWhenSuccessAsync<TResult, T1>(
        this Task<ResultBuilder> task, 
        string key, 
        Func<T1, Task<IResult<TResult>>> func,
        Func<ResultBuilder.ResultBuilderContext, T1> getArg1)
    {
        var builder = await task;
        return await builder.DoWhenSuccessAsync(key, func, getArg1);
    }
    
    public static async Task<ResultBuilder> DoWhenSuccessAsync<TResult, T1, T2>(
        this Task<ResultBuilder> task, 
        string key, 
        Func<T1, T2, Task<IResult<TResult>>> func,
        Func<ResultBuilder.ResultBuilderContext, T1> getArg1,
        Func<ResultBuilder.ResultBuilderContext, T2> getArg2)
    {
        var builder = await task;
        return await builder.DoWhenSuccessAsync(key, func, getArg1, getArg2);
    }
    
    public static async Task<ResultBuilder> DoWhenSuccessAsync<TResult, T1, T2, T3>(
        this Task<ResultBuilder> task, 
        string key, 
        Func<T1, T2, T3, Task<IResult<TResult>>> func,
        Func<ResultBuilder.ResultBuilderContext, T1> getArg1,
        Func<ResultBuilder.ResultBuilderContext, T2> getArg2,
        Func<ResultBuilder.ResultBuilderContext, T3> getArg3)
    {
        var builder = await task;
        return await builder.DoWhenSuccessAsync(key, func, getArg1, getArg2, getArg3);
    }
    
    public static async Task<ResultBuilder> OnSuccess(this Task<ResultBuilder> task, Action<ResultBuilder.ResultBuilderContext> action)
    {
        var builder = await task;
        return builder.OnSuccess(action);
    }
}