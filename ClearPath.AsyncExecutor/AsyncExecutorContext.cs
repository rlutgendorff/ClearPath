using ClearPath.Results;
using FluentValidation;

namespace ClearPath.AsyncExecutor;

public class AsyncExecutorContext
{
    private readonly Dictionary<string, Task> _context = new();
    private readonly Dictionary<string, IValidator> _validators = new();
    private readonly Dictionary<string, object> _stepResultCache = new();

    public Task<Result<T>> Get<T>(string key)
    {
        var combinedKey = GetKey<T>(key);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<Result<T>>)value;
        }

        throw new KeyNotFoundException($"Type {typeof(T).Name} not found for key {key} in executor context.");
    }

    public Task<Result> Get(string key)
    {
        var combinedKey = GetKey<Result>(key);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<Result>)value;
        }

        throw new KeyNotFoundException($"Type IResult not found for key {key} in executor context.");
    }

    public void Set<T>(string key, Task<Result<T>> value)
    {
        var combinedKey = GetKey<T>(key);

        _context[combinedKey] = value;
    }
    
    public void Set(string key, Task<Result> value)
    {
        var combinedKey = GetKey<Result>(key);
        _context[combinedKey] = value;
    }
    
    public Task[] GetAllTasks()
    {
        return _context.Values.ToArray();
    }

    public void AddValidator<T>(IValidator<T> validator)
    {
        var combinedKey = GetKey<T>();
        _validators[combinedKey] = validator;
    }

    public IValidator<T>? GetValidator<T>()
    {
        var combinedKey = GetKey<T>();

        if (_validators.TryGetValue(combinedKey, out var val))
            return val as IValidator<T>;
        return null;
    }

    public void SetCacheItem<T>(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        
        var combinedKey = GetKey<T>(key);
        _stepResultCache[combinedKey] = value;
    }
    
    public T? GetCacheItem<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        
        var combinedKey = GetKey<T>(key);
        if (_stepResultCache.TryGetValue(combinedKey, out var value))
        {
            return (T)value;
        }
        return default;
    }

    public bool TryGetCacheItem<T>(string key, out T? cachedItem)
    {
        cachedItem = default;
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var combinedKey = GetKey<T>(key);
        if (_stepResultCache.TryGetValue(combinedKey, out var value))
        {
            cachedItem = (T)value;
            return true;
        }
        return false;
    }


    private static string GetKey<T>(string key)
    {
        return $"{typeof(T).FullName}_{key}";
    }
    
    private static string GetKey<T>()
    {
        return typeof(T).FullName ?? throw new InvalidOperationException("Type name cannot be null.");
    }
}