using System.Reflection;
using ClearPath.Slim;

namespace ClearPath.AsyncExecutor;

public class FluentExecutor
{
    private readonly AsyncExecutorContext _context = new();
    private bool _hasFailed;
    private string _failedStepName;
    private string _failedStepError;
    private readonly List<string> _stepOrder = new();

    public string FailedStepName => _failedStepName;
    public string FailedStepError => _failedStepError;

    private FluentExecutor() { }

    public static FluentExecutor StartWith<T>(string key, T value)
    {
        var executor = new FluentExecutor();
        // Immediately wrap the value in a successful IResult<T> and a completed Task
        executor._context.SetKeyed(key, Task.FromResult<IResult<T>>(Result<T>.Ok(value)));
        return executor;
    }

    public FluentExecutor AddKeyedVariable<T>(string key, IResult<T> result)
    {
        if (_hasFailed) return this;
        _context.SetKeyed(key, Task.FromResult(result));
        if (!result.IsSuccess)
        {
            _hasFailed = true;
            _failedStepName ??= $"AddKeyedVariable:{key}";
            _failedStepError ??= result.Error;
        }
        return this;
    }

    public FluentExecutor AddKeyedVariable<T>(string key, Task<IResult<T>> resultTask)
    {
        if (_hasFailed) return this;
        _context.SetKeyed(key, resultTask);
        return this;
    }

    // Overloads for up to 3 parameters (expand as needed)
    public FluentExecutor Then<T1, TOut>(Func<T1, Task<IResult<TOut>>> stepFunc)
        => ThenInternal(stepFunc);

    public FluentExecutor Then<T1, T2, TOut>(Func<T1, T2, Task<IResult<TOut>>> stepFunc)
        => ThenInternal(stepFunc);

    public FluentExecutor Then<T1, T2, T3, TOut>(Func<T1, T2, T3, Task<IResult<TOut>>> stepFunc)
        => ThenInternal(stepFunc);

    private FluentExecutor ThenInternal(Delegate stepFunc)
    {
        if (_hasFailed) return this;

        var method = stepFunc.GetMethodInfo();
        var parameters = method.GetParameters();
        var returnType = method.ReturnType.GetGenericArguments().First(); // IResult<T>
        var outputType = returnType.GetGenericArguments().First();
        var stepName = method.Name;

        // Compose the task but do not start it yet (so downstream steps can consume it by key or type)
        Task<IResult<object>> stepTask = Task.Run(async () =>
        {
            // Resolve each parameter by its name (keyed lookup) and type
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                Task<IResult<object>> paramTask;
                try
                {
                    paramTask = ResolveParameter(p.ParameterType, p.Name);
                }
                catch (Exception ex)
                {
                    MarkFailure(stepName, ex.Message);
                    return Result<object>.Fail(ex.Message);
                }
                var result = await paramTask.ConfigureAwait(false);
                if (!result.IsSuccess)
                {
                    MarkFailure(stepName, result.Error);
                    return Result<object>.Fail(result.Error);
                }
                args[i] = result.Value;
            }

            // Invoke the step
            var resultTask = (Task)stepFunc.DynamicInvoke(args);
            await resultTask.ConfigureAwait(false);
            var resultProp = resultTask.GetType().GetProperty("Result");
            var resultObj = resultProp.GetValue(resultTask);
            var isSuccess = (bool)resultObj.GetType().GetProperty("IsSuccess").GetValue(resultObj);
            var value = resultObj.GetType().GetProperty("Value").GetValue(resultObj);
            var error = (string)resultObj.GetType().GetProperty("Error").GetValue(resultObj);

            if (!isSuccess)
            {
                MarkFailure(stepName, error);
                return Result<object>.Fail(error);
            }
            return (IResult<object>) Result<object>.Ok(value);
        });

        // Store the step in context by type and by name (so downstream steps can find it by parameter name or type)
        _context.Set<Task<IResult<object>>>(stepTask); // by type (object is the generic box, but see below)
        _context.SetKeyed(stepName, stepTask); // by step name

        // Also store by output type for by-type lookups:
        var setGenericMethod = typeof(AsyncExecutorContext)
            .GetMethod("Set", new[] { typeof(Task<IResult<object>>) })
            .MakeGenericMethod(outputType);

        // Create a casted Task<IResult<T>>
        var castedTask = CastTask(stepTask, outputType);
        setGenericMethod.Invoke(_context, new object[] { castedTask });

        _stepOrder.Add(stepName);
        return this;
    }

    // Helper for casting a Task<IResult<object>> to Task<IResult<T>>
    private static object CastTask(Task<IResult<object>> task, Type t)
    {
        // Use Task.ContinueWith to cast the result
        var resultType = typeof(IResult<>).MakeGenericType(t);
        var continueWith = typeof(TaskExtensions).GetMethod("CastResultTask").MakeGenericMethod(t);
        return continueWith.Invoke(null, new object[] { task });
    }

    private void MarkFailure(string stepName, string error)
    {
        if (!_hasFailed)
        {
            _hasFailed = true;
            _failedStepName = stepName;
            _failedStepError = error;
        }
    }

    // Resolve parameter using context by parameter name (keyed) then by type
    private Task<IResult<object>> ResolveParameter(Type paramType, string paramName)
    {
        // Try keyed lookup
        var getKeyed = typeof(AsyncExecutorContext).GetMethod("GetKeyed").MakeGenericMethod(paramType);
        try
        {
            var result = getKeyed.Invoke(_context, new object[] { paramName });
            return ConvertTaskToObjectResult(result, paramType);
        }
        catch { /* fallback to type-based */ }

        // Try by type
        var getByType = typeof(AsyncExecutorContext).GetMethod("Get").MakeGenericMethod(paramType);
        try
        {
            var result = getByType.Invoke(_context, null);
            return ConvertTaskToObjectResult(result, paramType);
        }
        catch { }

        throw new Exception($"Parameter {paramName} of type {paramType.Name} not found in context.");
    }

    private Task<IResult<object>> ConvertTaskToObjectResult(object taskObj, Type t)
    {
        var task = (Task)taskObj;
        var tcs = new TaskCompletionSource<IResult<object>>();
        task.ContinueWith(tk =>
        {
            var resultProp = tk.GetType().GetProperty("Result");
            var resultVal = resultProp.GetValue(tk);
            var isSuccess = (bool)resultVal.GetType().GetProperty("IsSuccess").GetValue(resultVal);
            var value = resultVal.GetType().GetProperty("Value").GetValue(resultVal);
            var error = (string)resultVal.GetType().GetProperty("Error").GetValue(resultVal);
            if (isSuccess)
                tcs.SetResult(Result<object>.Ok(value));
            else
                tcs.SetResult(Result<object>.Fail(error));
        });
        return tcs.Task;
    }

    public async Task<IResult<object>> GetFinalResultAsync()
    {
        if (_hasFailed)
            return Result<object>.Fail(_failedStepError ?? "Failed step: " + _failedStepName);

        if (_stepOrder.Count > 0)
        {
            // Try to get the last step's result by name
            var lastStep = _stepOrder.Last();
            var getKeyed = typeof(AsyncExecutorContext).GetMethod("GetKeyed").MakeGenericMethod(typeof(object));
            var taskObj = getKeyed.Invoke(_context, new object[] { lastStep });
            var task = (Task<IResult<object>>)taskObj;
            return await task.ConfigureAwait(false);
        }
        return Result<object>.Fail("No result produced");
    }
}

// Helper extension for result casting
public static class TaskExtensions
{
    public static async Task<IResult<T>> CastResultTask<T>(Task<IResult<object>> source)
    {
        var result = await source.ConfigureAwait(false);
        if (result.IsSuccess)
            return Result<T>.Ok((T)result.Value);
        return Result<T>.Fail(result.Error);
    }
}