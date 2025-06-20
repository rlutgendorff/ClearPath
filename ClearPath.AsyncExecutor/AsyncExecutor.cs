using System.Reflection;
using ClearPath.AsyncExecutor.Errors;
using ClearPath.Reasons;
using ClearPath.Results;
using FluentValidation;
using Polly;

namespace ClearPath.AsyncExecutor;

public class AsyncExecutor
{
    private readonly AsyncExecutorContext _context = new();

    private IAsyncPolicy? _policy;
    private AsyncExecutorEvents? _events;
    
    private bool HasFailed => _stepResults.Any(s => s is { IsSuccess: false });
    

    private readonly List<AsyncStepResult> _stepResults = new();
    public IReadOnlyList<AsyncStepResult> StepResults => _stepResults.AsReadOnly();

    private bool _outputCachingEnabled;

    private AsyncExecutor() {}

    internal AsyncExecutor(AsyncExecutorContext context)
    {
        _context = context;
    }

    #region StartWith

    public static AsyncExecutor StartWith<T>(string key, T value, AsyncExecutorEvents? events = null, IAsyncPolicy? policy = null)
    {
        return StartWith<T>(key, Result.Ok(value), events);
    }

    public static AsyncExecutor StartWith<T>(string key, Result<T> result, AsyncExecutorEvents? events = null, IAsyncPolicy? policy = null)
    {
        return StartWith(key, Task.FromResult(result), events);
    }

    public static AsyncExecutor StartWith<T>(string key, Task<Result<T>> resultTask, AsyncExecutorEvents? events = null, IAsyncPolicy? policy = null)
    {
        var executor = new AsyncExecutor();
        executor.TrackResult(key, resultTask);
        executor.AddKeyedVariable("executorContext", executor._context);
        executor._events = events;
        executor._policy = policy;
        return executor;
    }

    #endregion

    #region AddKeyedVariable

    public AsyncExecutor AddKeyedVariable<T>(string key, T value)
    {
        return AddKeyedVariable<T>(key, Result.Ok(value));
    }

    public AsyncExecutor AddKeyedVariable<T>(string key, Result<T> result)
    {
        return AddKeyedVariable(key, Task.FromResult(result));
    }

    public AsyncExecutor AddKeyedVariable<T>(string key, Task<Result<T>> resultTask)
    {
        if (HasFailed) return this;
        _context.Set(key, resultTask);
        return this;
    }

    #endregion

    #region Then

    public Task<AsyncExecutor> Then<TOut>(string key, Func<CancellationToken, Task<Result<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return Task.FromResult(this);

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return Task.FromResult(this);
            }

            Task<Result<TOut>> result = null!;
            if(policy != null)
                policy.ExecuteAsync(() => stepFunc(cancellationToken));
            else if(_policy != null)
                _policy.ExecuteAsync(() => stepFunc(cancellationToken));
            else
            {
                result = stepFunc(cancellationToken);
            }


            TrackResult(key, result, stepFunc.Method.Name);
            return Task.FromResult(this);
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }

    }

    public async Task<AsyncExecutor> Then<T1, TOut>(string key, Func<T1, CancellationToken, Task<Result<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return this;
            }

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;

            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Then<T1, T2, TOut>(string key, Func<T1, T2, CancellationToken, Task<Result<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return this;
            }

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;

            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, par2.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Then<T1, T2, T3, TOut>(string key, Func<T1, T2, T3, CancellationToken, Task<Result<TOut>>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return this;
            }

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;
            var par3 = await GetParameter<T3>(parameters[2]);
            if (!par3.IsSuccess) return this;
            
            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken);
            }

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

    #region ThenWithCompensation

    public Task<AsyncExecutor> ThenWithCompensation<TOut>(string key, Func<CancellationToken, Task<Result<TOut>>> stepFunc, string compensationKey, Func<AsyncExecutorContext, Task<Result>> compensationFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return Task.FromResult(this);

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return Task.FromResult(this);
            }

            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(cancellationToken));
            else
            {
                result = stepFunc(cancellationToken);
            }
            
            TrackResult(key, result, stepFunc.Method.Name, compensationKey, compensationFunc);
            return Task.FromResult(this);
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }

    }

    public async Task<AsyncExecutor> ThenWithCompensation<T1, TOut>(string key, Func<T1, CancellationToken, Task<Result<TOut>>> stepFunc, string compensationKey, Func<AsyncExecutorContext, Task<Result>> compensationFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return this;
            }

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;

            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name, compensationKey, compensationFunc);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> ThenWithCompensation<T1, T2, TOut>(string key, Func<T1, T2, CancellationToken, Task<Result<TOut>>> stepFunc, string compensationKey, Func<AsyncExecutorContext, Task<Result>> compensationFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return this;
            }

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;

            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, par2.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name, compensationKey, compensationFunc);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> ThenWithCompensation<T1, T2, T3, TOut>(string key, Func<T1, T2, T3, CancellationToken, Task<Result<TOut>>> stepFunc, string compensationKey, Func<AsyncExecutorContext, Task<Result>> compensationFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (HasFailed) return this;

            _events?.OnStepStart?.Invoke(key);

            if (_outputCachingEnabled && _context.TryGetCacheItem<TOut>(key, out var cachedItem))
            {
                TrackResult(key, Task.FromResult<Result>(Result.Ok(cachedItem)), stepFunc.Method.Name);
                return this;
            }

            var method = stepFunc.GetMethodInfo();
            var parameters = method.GetParameters();

            var par1 = await GetParameter<T1>(parameters[0]);
            if (!par1.IsSuccess) return this;
            var par2 = await GetParameter<T2>(parameters[1]);
            if (!par2.IsSuccess) return this;
            var par3 = await GetParameter<T3>(parameters[2]);
            if (!par3.IsSuccess) return this;

            Task<Result<TOut>> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name, compensationKey, compensationFunc);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    #endregion

    #region ThenIf

    public async Task<AsyncExecutor> ThenIf<TOut>(
        string key, 
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,  
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<CancellationToken, Task<Result<TOut>>> stepFunc, 
        IAsyncPolicy? policy = null, 
        CancellationToken cancellationToken = default)
    {
        if (HasFailed || !ifPredicate(_context, cancellationToken) || skipIfPredicate(_context, cancellationToken)) return this;

        return await Then(key, stepFunc, policy, cancellationToken);
    }

    public async Task<AsyncExecutor> ThenIf<T1, TOut>(
        string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<T1, CancellationToken, Task<Result<TOut>>> stepFunc, 
        IAsyncPolicy? policy = null, 
        CancellationToken cancellationToken = default)
    {
        if (HasFailed || !ifPredicate(_context, cancellationToken) || skipIfPredicate(_context, cancellationToken)) return this;

        return await Then(key, stepFunc, policy, cancellationToken);
    }

    public async Task<AsyncExecutor> ThenIf<T1, T2, TOut>(
        string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<T1, T2, CancellationToken, Task<Result<TOut>>> stepFunc, 
        IAsyncPolicy? policy = null, 
        CancellationToken cancellationToken = default)
    {
        if (HasFailed || !ifPredicate(_context, cancellationToken) || skipIfPredicate(_context, cancellationToken)) return this;

        return await Then(key, stepFunc, policy, cancellationToken);
    }

    public async Task<AsyncExecutor> ThenIf<T1, T2, T3, TOut>(
        string key,
        Func<AsyncExecutorContext, CancellationToken, bool> ifPredicate,
        Func<AsyncExecutorContext, CancellationToken, bool> skipIfPredicate,
        Func<T1, T2, T3, CancellationToken, Task<Result<TOut>>> stepFunc, 
        IAsyncPolicy? policy = null, 
        CancellationToken cancellationToken = default)
    {
        if (HasFailed || !ifPredicate(_context, cancellationToken) || skipIfPredicate(_context, cancellationToken)) return this;

        return await Then(key, stepFunc, policy, cancellationToken);
    }

    #endregion

    #region Do

    public Task<AsyncExecutor> Do(string key, Func<CancellationToken, Task<Result>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (HasFailed) return Task.FromResult(this);

            _events?.OnStepStart?.Invoke(key);

            var method = stepFunc.GetMethodInfo();

            Task<Result> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(cancellationToken));
            else
            {
                result = stepFunc(cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name);
            return Task.FromResult(this);
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Do<T1>(string key, Func<T1, CancellationToken, Task<Result>> stepFunc, IAsyncPolicy? policy = null, CancellationToken cancellationToken = default)
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

            Task<Result> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Do<T1, T2>(
        string key, Func<T1, T2, CancellationToken, Task<Result>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
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

            Task<Result> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, par2.Value, cancellationToken);
            }

            TrackResult(key, result, stepFunc.Method.Name);
            return this;
        }
        catch (Exception ex)
        {
            _events?.OnException?.Invoke(key, ex);
            throw;
        }
    }

    public async Task<AsyncExecutor> Do<T1, T2, T3>(
        string key, Func<T1, T2, T3, CancellationToken, Task<Result>> stepFunc,
        IAsyncPolicy? policy = null,
        CancellationToken cancellationToken = default)
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

            Task<Result> result = null!;
            if (policy != null)
                policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken));
            else if (_policy != null)
                _policy.ExecuteAsync(() => stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken));
            else
            {
                result = stepFunc(par1.Value, par2.Value, par3.Value, cancellationToken);
            }

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

    #region OnFailure

    public async Task<AsyncExecutor> OnFailure(Func<AsyncExecutorContext, Task> onFailure)
    {
        if (HasFailed) await onFailure(_context);

        return this;
    }

    #endregion

    public AsyncExecutor WithEvents(AsyncExecutorEvents events)
    {
        _events = events;
        return this;
    }

    public AsyncExecutor WithPolicy(IAsyncPolicy policy)
    {
        _policy = policy;
        return this;
    }

    public AsyncExecutor WithValidator<T>(IValidator<T> validator)
    {
        _context.AddValidator(validator);
        return this;
    }

    public async Task<AsyncExecutor> FinishAll(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(_context.GetAllTasks().Select(t => t.WaitAsync(cancellationToken)));
        return this;
    }

    public Task<Result> GetResult()
    {
        var failedSteps = _stepResults.Where(s => s is { IsSuccess: false, Errors: not null }).ToList();

        if (failedSteps.Count > 0)
        {
            var allErrors = failedSteps.SelectMany(s => s.Errors!).ToList();
            return Task.FromResult<Result>(Result.Fail(allErrors));
        }

        return Task.FromResult<Result>(Result.Ok());
    }

    public async Task<Result<T>> GetResult<T>(string key)
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

    public AsyncExecutor Map<TIn, TOut>(string sourceKey, Func<TIn, TOut> mapFunc, string? destKey = null)
    {
        // Get the source value as a task
        var sourceTask = _context.Get<TIn>(sourceKey);

        // Create a new task that maps the result
        var mappedTask = sourceTask.ContinueWith(task =>
        {
            if (!task.Result.IsSuccess)
                return Result<TOut>.Fail(task.Result.Errors);

            try
            {
                var mappedValue = mapFunc(task.Result.Value);
                return Result.Ok(mappedValue);
            }
            catch (Exception ex)
            {
                // Replace this with your own error/result logic
                return Result<TOut>.Fail([new MappingError { Message = ex.Message }]);
            }
        });

        var targetKey = destKey ?? sourceKey;
        _context.Set(targetKey, mappedTask);

        // Add to step results (metadata) for observability
        _stepResults.Add(new AsyncStepResult
        {
            Key = targetKey,
            MethodName = $"Map({sourceKey}→{targetKey})",
            Errors = null // Will be filled in by task if needed
        });

        return this;
    }

    public AsyncExecutor MapAsync<TIn, TOut>(string sourceKey, Func<TIn, Task<TOut>> mapFunc, string? destKey = null)
    {
        var sourceTask = _context.Get<TIn>(sourceKey);

        var mappedTask = sourceTask.ContinueWith(async t =>
        {
            if (!t.Result.IsSuccess)
                return Result<TOut>.Fail(t.Result.Errors);

            try
            {
                var mappedValue = await mapFunc(t.Result.Value);
                return Result.Ok(mappedValue);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Fail([new MappingError{Message = ex.Message}]);
            }
        }).Unwrap();

        var targetKey = destKey ?? sourceKey;
        _context.Set(targetKey, mappedTask);

        _stepResults.Add(new AsyncStepResult
        {
            Key = targetKey,
            MethodName = $"MapAsync({sourceKey}→{targetKey})",
            Errors = null
        });

        return this;
    }

    public AsyncExecutor EnableOutputCaching()
    {
        _outputCachingEnabled = true;
        return this;
    }

    public async Task<AsyncExecutor> Group(
        string groupKey,
        Func<AsyncExecutor, Task> groupSteps,
        Func<AsyncExecutor, Task>? onFailure = null)
    {
        var groupExecutor = new AsyncExecutor(_context)
        {
            _events = _events
        };

        await groupSteps(groupExecutor);
        await groupExecutor.FinishAll();

        // Import context and step metadata/results
        foreach (var result in groupExecutor.StepResults)
            _stepResults.Add(result);
        // (Optionally merge groupExecutor.Context as well)

        if (!(await groupExecutor.GetResult()).IsSuccess && onFailure != null)
            await onFailure(groupExecutor);

        return this;
    }


    private void TrackResult<T>(
        string key,
        Task<Result<T>> result,
        string? methodName = null,
        string? compensationKey = null,
        Func<AsyncExecutorContext, Task<Result>>? compensationFunc = null)
    {
        if (_stepResults.Any(r => r.Key == key))
            throw new InvalidOperationException($"Step key '{key}' is already used in the executor. All step keys must be unique.");

        var stepResult = new AsyncStepResult
        {
            Key = key,
            MethodName = methodName,
            CompensationKey = compensationKey,
            Compensation = compensationFunc
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


    private void TrackResult(
        string key, Task<Result> result, 
        string? methodName = null, 
        string? compensationKey = null, 
        Func<AsyncExecutorContext, Task<Result>>? compensationFunc = null)
    {
        if (_stepResults.Any(r => r.Key == key))
            throw new InvalidOperationException($"Step key '{key}' is already used in the executor. All step keys must be unique.");

        var stepResult = new AsyncStepResult
        {
            Key = key,
            MethodName = methodName,
            CompensationKey = compensationKey,
            Compensation = compensationFunc
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


    private async Task<Result<T>> GetParameter<T>(ParameterInfo info)
    {
        var result = await GetValue<T>(info);
        
        var validator = _context.GetValidator<T>();
        
        if (result.IsSuccess && validator != null)
        {
            var validationResult = await validator.ValidateAsync(result.Value!);
            
            if (!validationResult.IsValid)
            {
                return Result<T>.Fail(new ValidationError
                {
                    Message = $"{validationResult.Errors.Count} validation error(s) for parameter '{info.Name}'",
                    Reasons = validationResult.Errors.Select(x=> new ValidationError{Message = x.ErrorMessage}).Cast<IError>().ToList()
                });
            }
        }

        return result;
    }


    private Task<Result<T>> GetValue<T>(ParameterInfo parameter)
    {
        var key = GetKey(parameter);
        return _context.Get<T>(key);
    }

    private string GetKey(ParameterInfo parameter)
    {
        var attr = parameter.GetCustomAttribute<KeyedByAttribute>();
        return attr?.Key ?? parameter.Name!;
    }

    public async Task CompensateAll()
    {
        // Reverse through only successful steps with compensation registered
        foreach (var step in _stepResults.Where(s => s is { IsSuccess: true, Compensation: not null }).Reverse())
        {
            _events?.OnCompensationStart?.Invoke(step.Key);

            var compensationResult = await step.Compensation!(_context);
            if (compensationResult.IsSuccess)
            {
                _events?.OnCompensationSuccess?.Invoke(step.Key);
            }
            else
            {
                _events?.OnCompensationFailure?.Invoke(step.Key, compensationResult.Errors);
                
            }
        }
    }
}