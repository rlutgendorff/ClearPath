using ClearPath.Slim;

namespace ClearPath.AsyncExecutor;

public class AsyncExecutorContext
{
    private readonly Dictionary<string, object> _context = new();

    public Task<IResult<T>> Get<T>()
    {
        var typeName = typeof(T).FullName;
        if (_context.TryGetValue(typeName, out var value))
        {
            return (Task<IResult<T>>)value;
        }
        throw new KeyNotFoundException($"Type {typeName} not found in executor context.");
    }

    public Task<IResult<T>> GetKeyed<T>(string key)
    {
        var combinedKey = GetKey<T>(key);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<IResult<T>>)value;
        }

        throw new KeyNotFoundException($"Type {typeof(T).Name} not found for key {key} in executor context.");
    }

  

    public void Set<T>(Task<IResult<T>> value)
    {
        _context[typeof(T).FullName] = value;
    }


    public void SetKeyed<T>(string key, Task<IResult<T>> value)
    {
        var combinedKey = GetKey<T>(key);

        _context[combinedKey] = value;
    }

    private static string GetKey<T>(string key)
    {
        return $"{typeof(T).FullName}_{key}";
    }
}