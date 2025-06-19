using ClearPath.Results;

namespace ClearPath.DelegateExecutor;

public class DelegateExecutorContext
{
    private readonly Dictionary<string, Task> _context = new();

    public Task<IResult> Get(string key, Type type)
    {
        var combinedKey = GetKey(key, type);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<IResult>)value;
        }

        throw new KeyNotFoundException($"Type {type.Name} not found for key {key} in executor context.");
    }

    public Task<IResult> Get(string key)
    {
        var combinedKey = GetKey(key,typeof(IResult));

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return (Task<IResult>)value;
        }

        throw new KeyNotFoundException($"Type IResult not found for key {key} in executor context.");
    }

    public void Set(string key, Task value, Type type)
    {
        var combinedKey = GetKey(key, type);

        _context[combinedKey] = value;
    }

    public void Set(string key, Task<IResult> value)
    {
        var combinedKey = GetKey(key, typeof(IResult));
        _context[combinedKey] = value;
    }

    public Task[] GetAllTasks()
    {
        return _context.Values.ToArray();
    }

    private static string GetKey(string key, Type type)
    {
        return $"{type.FullName}_{key}";
    }
}