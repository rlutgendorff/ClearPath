using ClearPath.Reasons;
using ClearPath.Results;
using System.Reflection;

namespace ClearPath.DelegateExecutor;

public class DelegateExecutor
{
    private readonly Dictionary<string, object> _results = new();
    private readonly List<StepResult> _steps = new();
    private bool HasFailed => _steps.Any(s => !s.IsSuccess);

    public static DelegateExecutor StartWith(string key, object value)
    {
        var exec = new DelegateExecutor();
        exec._results[key] = WrapInResult(value);
        return exec;
    }

    public async Task<DelegateExecutor> Then(Delegate @delegate)
    {
        if (HasFailed) return this;

        var method = @delegate.GetMethodInfo();
        var returnKeyAttr = method.GetCustomAttribute<ReturnTypeKeyAttribute>();
        if (returnKeyAttr == null)
            throw new InvalidOperationException($"Missing ReturnTypeKey on method {method.Name}");

        var parameters = method.GetParameters();
        var args = new List<object>();
        var paramKeys = new List<string>();

        foreach (var parameterInfo in parameters)
        {
            var param = await GetParameter(parameterInfo);
            paramKeys.Add(parameterInfo.Name!);

            if (param == null)
            {
                args.Add(null!);
            }
            else if (param.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResult<>)))
            {
                var valueProp = param.GetType().GetProperty("Value");
                args.Add(valueProp?.GetValue(param));
            }
            else
            {
                args.Add(param);
            }
        }

        var resolvedTask = Task.Run(async () =>
        {
            var result = @delegate.DynamicInvoke(args.ToArray());

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                var resultProp = task.GetType().GetProperty("Result");
                var value = resultProp?.GetValue(task);
                return WrapInResult(value);
            }

            return WrapInResult(result);
        });

        _steps.Add(new StepResult
        {
            ReturnKey = returnKeyAttr.Key,
            ResolvedTask = resolvedTask,
            MethodName = method.Name,
            ParameterKeys = paramKeys
        });

        return this;
    }

    public async Task<object?> Result(string key)
    {
        if (_results.ContainsKey(key))
            return _results[key];

        var step = _steps.FirstOrDefault(s => s.ReturnKey == key);
        if (step == null)
            throw new InvalidOperationException($"No step found for key: {key}");

        var output = await step.ResolvedTask;
        _results[key] = output;
        return output;
    }

    private async Task<object?> GetParameter(ParameterInfo parameterInfo)
    {
        var key = parameterInfo.Name;
        if (string.IsNullOrEmpty(key)) return null;

        return await Result(key);
    }

    private static object WrapInResult(object? value)
    {
        if (value is null) return Activator.CreateInstance(typeof(Result<>).MakeGenericType(typeof(object)), value)!;

        var type = value.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>)) return value;
        if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResult<>))) return value;

        return Results.Result.Ok((dynamic)value);
    }
}

public static class DelegateExecutorExtensions
{
    public static async Task<DelegateExecutor> Then(this Task<DelegateExecutor> task, Delegate @delegate)
    {
        var executor = await task;

        return await executor.Then(@delegate);
    }
}