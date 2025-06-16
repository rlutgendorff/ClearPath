using ClearPath.Results;

namespace ClearPath.AsyncExecutor;

public class AsyncExecutorContext
{
    private readonly Dictionary<string, Task> _context = new();
    
    public Task<IResult<T>> Get<T>(string key)
    {
        var combinedKey = GetKey<T>(key);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<IResult<T>>)value;
        }

        throw new KeyNotFoundException($"Type {typeof(T).Name} not found for key {key} in executor context.");
    }

    public Task<IResult> Get(string key)
    {
        var combinedKey = GetKey<IResult>(key);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<IResult>)value;
        }

        throw new KeyNotFoundException($"Type IResult not found for key {key} in executor context.");
    }

    public void Set<T>(string key, Task<IResult<T>> value)
    {
        var combinedKey = GetKey<T>(key);

        _context[combinedKey] = value;
    }
    
    public void Set(string key, Task<IResult> value)
    {
        var combinedKey = GetKey<IResult>(key);
        _context[combinedKey] = value;
    }
    
    public Task[] GetAllTasks()
    {
        return _context.Values.ToArray();
    }

    private static string GetKey<T>(string key)
    {
        return $"{typeof(T).FullName}_{key}";
    }
}