using System.Reflection;
using ClearPath.Results;

namespace ClearPath.DelegateExecutor;

public class DelegateExecutorContext
{
    private readonly Dictionary<string, Task> _context = new();

    public Task<Result> Get(string key, Type type)
    {
        var combinedKey = GetKey(key, type);

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return value as Task<Result>;
        }

        throw new KeyNotFoundException($"Type {type.Name} not found for key {key} in executor context.");
    }

    public Task<Result> Get(string key)
    {
        var combinedKey = GetKey(key, typeof(Result));

        if (_context.TryGetValue(combinedKey, out var value))
        {
            return value as Task<Result>;
        }

        throw new KeyNotFoundException($"Type Result not found for key {key} in executor context.");
    }

    public void Set(string key, Task<Result> value)
    {
        var combinedKey = GetKey(key, typeof(Result));
        _context[combinedKey] = value;
    }
    
    public void Set<T>(string key, Task<Result<T>> value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        
        var combinedKey = GetKey(key, typeof(Result<T>));
        _context[combinedKey] = value;
    }
    
    public void Set(string key, object value, Type mainType)
    {
        var combinedKey = string.Empty;
        Task<Result> objectToStore;

        if (value is Task)
        {
            if (mainType.IsGenericType)
            {
                var type = mainType.GetGenericArguments()[0];

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    combinedKey = GetKey(key, type.GenericTypeArguments[0]);

                    var method = GetType().GetMethod("CastGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
                    objectToStore = (Task<Result>)method.MakeGenericMethod(type.GenericTypeArguments[0]).Invoke(this, [key, value]);
                }
                else if (type == typeof(Result))
                {
                    combinedKey = GetKey(key, typeof(Result));
                    objectToStore = (Task<Result>)value;
                }
                else if (type.Name == "VoidTaskResult") //For Task.CompletedTask
                {
                    combinedKey = GetKey(key, typeof(Result));
                    objectToStore = Task.FromResult(Result.Ok());
                }
                else
                {
                    combinedKey = GetKey(key, type);

                    var method = GetType().GetMethod("CastObject", BindingFlags.NonPublic | BindingFlags.Instance);
                    objectToStore = (Task<Result>)method.MakeGenericMethod(type).Invoke(this, [key, value]);
                }
            }
            else
            {
                combinedKey = GetKey(key, typeof(Result));
                objectToStore = Task.FromResult(Result.Ok());
                
            }
        }
        else if (value is Result result)
        {
            if (result.GetType().IsGenericType)
            {
                combinedKey = GetKey(key, result.GetType().GetGenericArguments()[0]);
                objectToStore = Task.FromResult(result);
            }
            else
            {
                combinedKey = GetKey(key, typeof(Result));
                objectToStore = Task.FromResult(result);
            }
        }
        else
        {
            combinedKey = GetKey(key, mainType);

            var r = typeof(Result);
            var method = r
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == "Ok" &&
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 1);

            var o = method.MakeGenericMethod(mainType).Invoke(null, [value]);

            objectToStore = Task.FromResult<Result>((dynamic)o);
        }
        
        _context.Add(combinedKey, objectToStore);
        
    }

    // DO NOT REMOVE: used with reflection
    private async Task<Result> CastObject<T>(string key, Task<T> task)
    {
        var value = await task;

        return Result.Ok(value);
    }

    // DO NOT REMOVE: used with reflection
    private async Task<Result> CastGeneric<T>(string key, Task<Result<T>> task)
    {
        var value = await task.ConfigureAwait(false);

        return value;
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