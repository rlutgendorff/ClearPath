using ClearPath.Reasons;
using ClearPath.Results;
using System.Reflection;

namespace ClearPath.DelegateExecutor;

public class DelegateExecutor
{
    private readonly DelegateExecutorContext _context = new();

    private DelegateExecutor() { }


    private bool HasFailed => _stepResults.Any(s => s is { IsSuccess: false });


    private readonly List<StepResult> _stepResults = new();

    public static DelegateExecutor StartWith<T>(string key, T value)
    {
        return StartWith<T>(key, Result.Ok(value));
    }

    public static DelegateExecutor StartWith<T>(string key, IResult<T> result)
    {
        return StartWith(key, Task.FromResult((IResult)result), typeof(T));
    }

    public static DelegateExecutor StartWith(string key, Task<IResult> resultTask, Type type)
    {
        var executor = new DelegateExecutor();
        executor.TrackResult(key, resultTask, type);
        executor.AddKeyedVariable("executorContext", executor._context);
        return executor;
    }

    #region AddKeyedVariable

    public DelegateExecutor AddKeyedVariable<T>(string key, T value)
    {
        return AddKeyedVariable(key, Result.Ok((object)value), typeof(T));
    }

    public DelegateExecutor AddKeyedVariable(string key, IResult result, Type type)
    {
        return AddKeyedVariable(key, Task.FromResult(result), type);
    }

    public DelegateExecutor AddKeyedVariable(string key, Task<IResult> resultTask, Type type)
    {
        if (HasFailed) return this;
        _context.Set(key, resultTask, type);
        return this;
    }

    #endregion

    public async Task<DelegateExecutor> Then(string key, Delegate @delegate, CancellationToken cancellationToken = default)
    {
        if (HasFailed) return this;

            
        var method = @delegate.GetMethodInfo();
        var parameters = method.GetParameters();

        var args = new List<object>();
            
        foreach (var parameterInfo in parameters)
        {
            var param = await GetParameter(parameterInfo);

            if (param.GetType().IsGenericType)
            {
                var result = ((dynamic)param).Value;
                args.Add(result);
            }
        }

        var response = @delegate.DynamicInvoke(args.ToArray());

        var returnType = @delegate.Method.ReturnType;
        
        if(response is Task<IResult> task)
            TrackResult(key, task, returnType);

        return this;
    }

    private void TrackResult(
        string key, Task<IResult> result,
        Type type,
        string? methodName = null,
        string? compensationKey = null,
        Func<DelegateExecutor, Task<IResult>>? compensationFunc = null)
    {
        if (_stepResults.Any(r => r.Key == key))
            throw new InvalidOperationException($"Step key '{key}' is already used in the executor. All step keys must be unique.");

        var stepResult = new StepResult
        {
            Key = key,
            MethodName = methodName,
        };
        _stepResults.Add(stepResult);

        _context.Set(key, result, type);

        result.ContinueWith(task =>
        {
            if (!task.Result.IsSuccess)
            {
                stepResult.Errors = task.Result.Errors;
            }
            else
            {
                stepResult.Errors = null;
            }
        });
    }

    private async Task<IResult> GetParameter(ParameterInfo info)
    {
        var result = await GetValue(info);

        return result;
    }
    
    private Task<IResult> GetValue(ParameterInfo parameter)
    {
        var key = parameter.Name; //GetKey(parameter);
        return _context.Get(key!, parameter.ParameterType);
    }

    // private string GetKey(ParameterInfo parameter)
    // {
    //     var attr = parameter.GetCustomAttribute<KeyedByAttribute>();
    //     return attr?.Key ?? parameter.Name!;
    // }


}