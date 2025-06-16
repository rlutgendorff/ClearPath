using System.Reflection;
using ClearPath.Reasons;
using ClearPath.Results;

namespace ClearPath.AsyncExecutor;

public class AsyncExecutor
{
    private readonly AsyncExecutorContext _context = new();
    private AsyncExecutorEvents? _events;
    
    private bool HasFailed => _stepResults.Any(s => s is { IsSuccess: false });

    private readonly List<AsyncStepResult> _stepResults = new();
    public IReadOnlyList<AsyncStepResult> StepResults => _stepResults.AsReadOnly();

    private AsyncExecutor() { }

    #region StartWith

    public static AsyncExecutor StartWith<T>(string key, T value, AsyncExecutorEvents? events = null)
    {
        return StartWith<T>(key, Result.Ok(value), events);
    }

    public static AsyncExecutor StartWith<T>(string key, IResult<T> result, AsyncExecutorEvents? events = null)
    {
        return StartWith(key, Task.FromResult(result), events);
    }

    public static AsyncExecutor StartWith<T>(string key, Task<IResult<T>> resultTask, AsyncExecutorEvents? events = null)
    {
        var executor = new AsyncExecutor();
        executor.TrackResult(key, resultTask);
        executor._events = events;
        return executor;
    }

    #endregion

    #region AddKeyedVariable

    public AsyncExecutor AddKeyedVariable<T>(string key, T value)
    {
        return AddKeyedVariable<T>(key, Result.Ok(value));
    }

    public AsyncExecutor AddKeyedVariable<T>(string key, IResult<T> result)
    {
        return AddKeyedVariable(key, Task.FromResult(result));
    }

    public AsyncExecutor AddKeyedVariable<T>(string key, Task<IResult<T>> resultTask)
    {
        if (HasFailed) return this;
        _context.Set(key, resultTask);
        return this;
    }

    #endregion

    #region Then

    public Task<AsyncExecutor> Then<TOut>(string key, Func<Task<IResult<TOut>>> stepFunc)
    {
        try
        {
            if (HasFailed) return Task.FromResult(this);

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var result = stepFunc();
            TrackResult(key, result);
            return Task.FromResult(this);
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }

    }

    public async Task<AsyncExecutor> Then<T1, TOut>(string key, Func<T1, Task<IResult<TOut>>> stepFunc)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;

            var result = stepFunc(par1.Value);
            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Then<T1, T2, TOut>(string key, Func<T1, T2, Task<IResult<TOut>>> stepFunc)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;

            var result = stepFunc(par1.Value, par2.Value);
            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Then<T1, T2, T3, TOut>(string key, Func<T1, T2, T3, Task<IResult<TOut>>> stepFunc)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;
            var par3 = await GetParameter<T3>(parameters[2]);
            if (!par3.IsSuccess) return this;


            var result = stepFunc(par1.Value, par2.Value, par3.Value);
            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    #endregion


    #region Do

    public Task<AsyncExecutor> Do(string key, Func<Task<IResult>> stepFunc)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (HasFailed) return Task.FromResult(this);

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var result = stepFunc();
            TrackResult(key, result, stepFunc.Method.Name);
            return Task.FromResult(this);
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Do<T1>(string key, Func<T1, Task<IResult>> stepFunc)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;

            var result = stepFunc(par1.Value);
            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Do<T1, T2>(string key, Func<T1, T2, Task<IResult>> stepFunc)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;

            var result = stepFunc(par1.Value, par2.Value);
            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Do<T1, T2, T3>(string key, Func<T1, T2, T3, Task<IResult>> stepFunc)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;
            var par3 = await GetParameter<T3>(parameters[2]);
            if (!par3.IsSuccess) return this;


            var result = stepFunc(par1.Value, par2.Value, par3.Value);
            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    #endregion
    
    public AsyncExecutor WithEvents(AsyncExecutorEvents events)
    {
        _events = events;
        return this;
    }

    public async Task<AsyncExecutor> FinishAll()
    {
        await Task.WhenAll(_context.GetAllTasks());
        return this;
    }

    public Task<IResult> GetResult()
    {
        var failedSteps = _stepResults.Where(s => s is { IsSuccess: false, Errors: not null }).ToList();

        if (failedSteps.Count > 0)
        {
            var allErrors = failedSteps.SelectMany(s => s.Errors!).ToList();
            return Task.FromResult<IResult>(Result.Fail(allErrors));
        }

        return Task.FromResult<IResult>(Result.Ok());
    }

    public async Task<IResult<T>> GetResult<T>(string key)
    {
        var failedSteps = _stepResults.Where(s => s is { IsSuccess: false, Errors: not null }).ToList();

        if (failedSteps.Count > 0)
        {
            var allErrors = failedSteps.SelectMany(s => s.Errors!).ToList();
            return Result<T>.Fail(allErrors);
        }
        
        var result = await _context.Get<T>(key);
        return result;
    }


    private void TrackResult<T>(string key, Task<IResult<T>> result, string? methodName = null)
    {
        if (_stepResults.Any(r => r.Key == key))
            throw new InvalidOperationException($"Step key '{key}' is already used in the executor. All step keys must be unique.");

        var stepResult = new AsyncStepResult
        {
            Key = key,
            MethodName = methodName
            // If you want, set MethodName here if available
        };
        _stepResults.Add(stepResult);

        _context.Set(key, result);

        result.ContinueWith(task =>
        {
            if (!task.Result.IsSuccess)
            {
                stepResult.Errors = task.Result.Errors;
                _events?.OnStepFailure?.Invoke(key, task.Result.Errors);
            }
            else
            {
                stepResult.Errors = null;
                _events?.OnStepSuccess?.Invoke(key);
            }
        });
    }


    private void TrackResult(string key, Task<IResult> result, string? methodName = null)
    {
        if (_stepResults.Any(r => r.Key == key))
            throw new InvalidOperationException($"Step key '{key}' is already used in the executor. All step keys must be unique.");

        var stepResult = new AsyncStepResult
        {
            Key = key,
            MethodName = methodName,
            Errors = null
        };
        _stepResults.Add(stepResult);

        _context.Set(key, result);

        result.ContinueWith(task =>
        {
            if (!task.Result.IsSuccess)
            {
                stepResult.Errors = task.Result.Errors;
                _events?.OnStepFailure?.Invoke(key, task.Result.Errors);
            }
            else
            {
                stepResult.Errors = null;
                _events?.OnStepSuccess?.Invoke(key);
            }
        });
    }


    private async Task<IResult<T>> GetParameter<T>(ParameterInfo info)
    {
        var result = await GetValue<T>(info);
        return result;
    }


    private Task<IResult<T>> GetValue<T>(ParameterInfo parameter)
    {
        var key = GetKey(parameter);
        return _context.Get<T>(key);
    }

    private string GetKey(ParameterInfo parameter)
    {
        var key = parameter.Name;


        /*if (string.IsNullOrEmpty(key))
        {
            key = parameter?
                .GetCustomAttribute<KeyedByValueAttribute>()?.Key;
        }*/

        return key;
    }
}