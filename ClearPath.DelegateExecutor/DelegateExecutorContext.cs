using ClearPath.Results;

namespace ClearPath.DelegateExecutor;

public class DelegateExecutorContext
{
    private readonly Dictionary<string, Task> _context = new();

    //public Task<IResult> Get(string key, Type type)
    //{
    //    var combinedKey = GetKey(key, type);

    //    if (_context.TryGetValue(combinedKey, out var value))
    //    {
    //        return (Task<IResult>)value;
    //    }

    //    throw new KeyNotFoundException($"Type {type.Name} not found for key {key} in executor context.");
    //}

    //public Task<IResult> Get(string key)
    //{
    //    var combinedKey = GetKey(key,typeof(IResult));

    //    if (_context.TryGetValue(combinedKey, out var value))
    //    {
    //        return (Task<IResult>)value;
    //    }

    //    throw new KeyNotFoundException($"Type IResult not found for key {key} in executor context.");
    //}

    /*public void Set(string key, Task value, Type type)
    {
        var combinedKey = GetKey(key, type);

        _context[combinedKey] = value;
    }

    public void Set(string key, Task<IResult> value)
    {
        var combinedKey = GetKey(key, typeof(IResult));
        _context[combinedKey] = value;
    }*/

    public void Set(string key, object value)
    {
        var combinedKey = string.Empty;
        Task objectToStore = null!;

        if (value is Task)
        {
            if (value.GetType().IsGenericType)
            {
                var type = value.GetType().GetGenericArguments()[0];
                if (type.GetInterfaces().Any(i => i == typeof(IResult)))
                {
                    combinedKey = "t0";
                    //TODO: Handle Task with Result
                }
                else if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResult<>)))
                {
                    combinedKey = "t1";
                    //TODO: Handle Task with Result
                }

                //TODO: Handle Task without Result
            }
            else
            {
                combinedKey = GetKey(key, typeof(Task));
                objectToStore = value as Task;
                
            }

            //TODO: Handle Task without Result
        }
        else if (value is IResult result)
        {
            if (result.GetType().IsGenericType)
            {
                combinedKey = GetKey(key, result.GetType().GetGenericArguments()[0]);
                objectToStore = Task.FromResult((dynamic)result);
            }
            else
            {
                combinedKey = GetKey(key, typeof(IResult));
                objectToStore = Task.FromResult(result);
            }
        }
        else
        {
            combinedKey = GetKey(key, value.GetType());
            objectToStore = Task.FromResult(Result.Ok((dynamic)value));
        }
        
        _context.Add(combinedKey, objectToStore);
        
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