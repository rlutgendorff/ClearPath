using ClearPath.Results;

namespace ClearPath.Builders;

public class ResultBuilder
{
    private readonly ResultBuilderContext _context = new();
    private readonly List<StepFailure> _failures = [];
    private ResultBuilderEvents? _events;

    private ResultBuilder() { }
    
    public static ResultBuilder StartWith<T>(string key, IResult<T> result, ResultBuilderEvents? events = null)
    {
        var builder = new ResultBuilder();
        builder._events = events;
        builder.TrackResult(key, result);
        return builder;
    }

    public static ResultBuilder StartWith<T>(string key, Func<IResult<T>> func, ResultBuilderEvents? events = null)
    {
        var builder = new ResultBuilder
        {
            _events = events
        };
        builder.TrackResult(key, func());
        return builder;
    }

    public ResultBuilder With<T>(string key, T value)
    {
        _context.Set(key, Result.Ok(value));
        return this;
    }

    public ResultBuilder WithEvents(ResultBuilderEvents events)
    {
        _events = events;
        return this;
    }

    public ResultBuilder DoWhenSuccess<T>(string key, Func<ResultBuilderContext, IResult<T>> func)
    {
        if (HasFailure) return this;

        _events?.OnStepStart?.Invoke(key);

        var result = func(_context);
        TrackResult(key, result);

        return this;
    }

    public async Task<ResultBuilder> DoWhenSuccessAsync<T>(
        string key, 
        Func<Task<IResult<T>>> func)
    {
        if (HasFailure) return this;
        _events?.OnStepStart?.Invoke(key);

        var result = await func();
        TrackResult(key, result);
        
        return this;
    }

    public async Task<ResultBuilder> DoWhenSuccessAsync<T>(
        string key, 
        Func<ResultBuilderContext, Task<IResult<T>>> func)
    {
        if (HasFailure) return this;

        _events?.OnStepStart?.Invoke(key);

        var result = await func(_context);
        TrackResult(key, result);

        return this;
    }
    
    public async Task<ResultBuilder> DoWhenSuccessAsync<TResult, T1>(
        string key,
        Func<T1, Task<IResult<TResult>>> func,
        Func<ResultBuilderContext, T1> getArg1)
    {
        if (HasFailure) return this;

        _events?.OnStepStart?.Invoke(key);

        var arg1 = getArg1(_context);
        var result = await func(arg1);
        TrackResult(key, result);

        return this;
    }
    
    public async Task<ResultBuilder> DoWhenSuccessAsync<TResult, T1, T2>(
        string key,
        Func<T1, T2, Task<IResult<TResult>>> func,
        Func<ResultBuilderContext, T1> getArg1,
        Func<ResultBuilderContext, T2> getArg2)
    {
        if (HasFailure) return this;

        _events?.OnStepStart?.Invoke(key);

        var arg1 = getArg1(_context);
        var arg2 = getArg2(_context);
        var result = await func(arg1, arg2);
        TrackResult(key, result);

        return this;
    }
    
    public async Task<ResultBuilder> DoWhenSuccessAsync<TResult, T1, T2, T3>(
        string key,
        Func<T1, T2, T3, Task<IResult<TResult>>> func,
        Func<ResultBuilderContext, T1> getArg1,
        Func<ResultBuilderContext, T2> getArg2,
        Func<ResultBuilderContext, T3> getArg3)
    {
        if (HasFailure) return this;

        _events?.OnStepStart?.Invoke(key);

        var arg1 = getArg1(_context);
        var arg2 = getArg2(_context);
        var arg3 = getArg3(_context);
        var result = await func(arg1, arg2, arg3);
        TrackResult(key, result);

        return this;
    }

    public ResultBuilder OnSuccess(string key, Action<ResultBuilderContext> action)
    {
        if (_context.TryGetValue(key, out var result) && result.IsSuccess)
        {
            action(_context);
        }
        return this;
    }
    
    public ResultBuilder OnSuccess(Action<ResultBuilderContext> action)
    {
        if (!HasFailure)
        {
            action(_context);
        }
        return this;
    }

    public ResultBuilder OnFailure(Action<ResultBuilderContext, List<StepFailure>> action)
    {
        if (_failures.Count != 0)
        {
            action(_context, _failures);
        }
        return this;
    }

    public ResultBuilder OnFailure(string failedKey, Func<ResultBuilderContext, IResult> fallbackFunc, string fallbackKey)
    {
        if (_failures.Any(f => f.Key == failedKey))
        {
            var fallback = fallbackFunc(_context);
            TrackResult(fallbackKey, fallback);
            return this;
        }

        return this;
    }

    public ResultBuilder RetryOnFailure(string key, Func<ResultBuilderContext, IResult> func, int maxAttempts = 3, int delayMs = 250)
    {
        if (_failures.Any(f => f.Key == key))
        {
            _failures.RemoveAll(f => f.Key == key);

            for (var i = 0; i < maxAttempts; i++)
            {
                var result = func(_context);
                if (result.IsSuccess)
                {
                    _context.Set(key, result);
                    return this;
                }

                Thread.Sleep(delayMs);
            }

            TrackResult(key, func(_context));
        }

        return this;
    }

    public async Task<ResultBuilder> RetryOnFailureAsync(string key, Func<ResultBuilderContext, Task<IResult>> func, int maxAttempts = 3, int delayMs = 250)
    {
        if (_failures.Any(f => f.Key == key))
        {
            _failures.RemoveAll(f => f.Key == key);

            for (int i = 0; i < maxAttempts; i++)
            {
                var result = await func(_context);
                if (result.IsSuccess)
                {
                    _context.Set(key, result);
                    return this;
                }

                await Task.Delay(delayMs);
            }

            TrackResult(key, await func(_context));
        }

        return this;
    }

    

    public IResult<T> Build<T>(string key) => _context.Get<T>(key);
    public IResult Build(string key) => _context.Get(key);
    public IResult<T>? BuildOnSuccess<T>(string key) => !HasFailure ? _context.Get<T>(key) : null;

    public bool HasFailure => _failures.Count > 0;

    public IReadOnlyList<StepFailure> GetFailures() => _failures;

    private void TrackResult(string key, IResult result)
    {
        _context.Set(key,result);

        if (result.IsFailed)
        {
            _failures.Add(new StepFailure(key, result.Errors));
            _events?.OnStepFailure?.Invoke(key, result.Errors);
        }
        else
        {
            _events?.OnStepSuccess?.Invoke(key);
        }
    }
    
    public class ResultBuilderContext
    {
        private readonly Dictionary<string, IResult> _values = new();

        public IResult Get(string key)
        {
            return _values.TryGetValue(key, out var result)
                ? result
                : throw new KeyNotFoundException($"No result found for key '{key}'");
        }
    
        public IResult<T> Get<T>(string key)
        {
            return _values.TryGetValue(key, out var result)
                ? (IResult<T>)result
                : throw new KeyNotFoundException($"No result found for key '{key}'");
        }

        public bool TryGetValue(string key, out IResult? result)
        {
            return _values.TryGetValue(key, out result);
        }
        
        public bool TryGetValue<T>(string key, out IResult<T>? result)
        {
            result = null;

            if (_values.TryGetValue(key, out var value) && value is Result<T> typedValue)
            {
                result = typedValue;
                return true;
            }

            return false;
        }

        internal void Set(string key, IResult result)
        {
            _values[key] = result;
        }
    }
} 