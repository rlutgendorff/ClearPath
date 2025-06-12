using ClearPath.Results;

namespace ClearPath.Executors;

public class FluentExecutorContext
{
    private readonly Dictionary<string, object> _context = new();

    public IResult<T> Get<T>()
    {
        var typeName = typeof(T).FullName;
        if (_context.TryGetValue(typeName, out var value))
        {
            return (IResult<T>)value;
        }
        throw new KeyNotFoundException($"Type {typeName} not found in executor context.");
    }

    public IResult<T> GetKeyed<T>(string key)
    {
        var combinedKey = GetKey<T>(key);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (IResult<T>)value;
        }

        throw new KeyNotFoundException($"Type {typeof(T).Name} not found for key {key} in executor context.");
    }

    public void Set<T>(IResult<T> value)
    {
        _context[typeof(T).FullName] = value;
    }
    
    public void SetKeyed<T>(string key, IResult<T> value)
    {
        var combinedKey = GetKey<T>(key);

        _context[combinedKey] = value;
    }
    
    private static string GetKey<T>(string key)
    {
        return $"{typeof(T).FullName}_{key}";
    }
}