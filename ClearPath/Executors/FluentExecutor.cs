using ClearPath.Builders;
using ClearPath.Reasons;
using ClearPath.Results;
using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using static System.Collections.Specialized.BitVector32;

namespace ClearPath.Executors;

public class FluentExecutor
{
    private readonly FluentExecutorContext _context = new();
    private readonly List<StepFailure> _failures = [];
    private FluentExecutorEvents? _events = new();

    private StepResult _result = StepResult.Ok();

    private FluentExecutor() { }
    
    public static FluentExecutor StartWith<T>(IResult<T> result)
    {
        var executor = new FluentExecutor();
        executor._context.Set(result);
        return executor;
    }
    
    public static FluentExecutor StartWith<T>(string key, IResult<T> result)
    {
        var executor = new FluentExecutor();
        executor._context.SetKeyed(key, result);
        return executor;
    }
    
    public FluentExecutor AddKeyedVariable<T>(string key, IResult<T> result)
    {
        _context.SetKeyed(key, result);
        return this;
    }

    public FluentExecutor Then<T>(Func<IResult<T>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);
        
        _result.Metadata.Steps++;
        
        var result = func();
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public async Task<FluentExecutor> Then<T>(Func<Task<IResult<T>>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);

        _result.Metadata.Steps++;

        var result = await func();
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public FluentExecutor Then<T1, T> (Func<T1, IResult<T>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);
        
        _result.Metadata.Steps++;

        var input = GetValue<T1>(func.Method.GetParameters()[0]);

        var result = func(input);
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public async Task<FluentExecutor> Then<T1, T>(Func<T1, Task<IResult<T>>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);

        _result.Metadata.Steps++;

        var input = GetValue<T1>(func.Method.GetParameters()[0]);
        
        var result = await func(input);
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public FluentExecutor Then<T1, T2, T>(Func<T1, T2, IResult<T>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);
        
        _result.Metadata.Steps++;
        
        var parameters = func.Method.GetParameters();

        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var result = func(input1, input2);
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public async Task<FluentExecutor> Then<T1, T2, T>(Func<T1, T2, Task<IResult<T>>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);

        _result.Metadata.Steps++;

        var parameters = func.Method.GetParameters();

        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var result = await func(input1, input2);
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public FluentExecutor Then<T1, T2, T3, T>(Func<T1, T2, T3, IResult<T>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);
        
        _result.Metadata.Steps++;

        var parameters = func.Method.GetParameters();

        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var input3 = GetValue<T3>(parameters[2]);
        var result = func(input1, input2, input3);
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public async Task<FluentExecutor> Then<T1, T2, T3, T>(Func<T1, T2, T3, Task<IResult<T>>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke(typeof(T).Name);

        _result.Metadata.Steps++;

        var parameters = func.Method.GetParameters();

        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var input3 = GetValue<T3>(parameters[2]);
        var result = await func(input1, input2, input3);
        TrackResult(key, result, func.Method.Name);
        return this;
    }

    public FluentExecutor Then(Action action, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        try
        {
            action();
        }
        catch (Exception e)
        {
            if (string.IsNullOrEmpty(key))
                key = action.Method.Name;

            TrackResult(key, StepResult.Fail(new Error($"Action failed: {e.Message}")));
        }

        return this;
    }

    public async Task<FluentExecutor> Then(Func<Task> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        try
        {
            await func();
        }
        catch (Exception e)
        {
            if (string.IsNullOrEmpty(key))
                key = func.Method.Name;

            TrackResult(key, StepResult.Fail(new Error($"Action failed: {e.Message}")));
        }

        return this;
    }

    public FluentExecutor Then<T1>(Action<T1> action, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");

        _result.Metadata.Steps++;

        var parameter = action.Method.GetParameters()[0];

        var input = GetValue<T1>(parameter);

        try
        {
            action(input);
        }
        catch (Exception e)
        {
            if (string.IsNullOrEmpty(key))
                key = action.Method.Name;

            TrackResult(key, StepResult.Fail(new Error($"Action failed: {e.Message}")));
        }
        return this;
    }

    public async Task<FluentExecutor> Then<T1>(Func<T1, Task<IResult>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        var parameter = func.Method.GetParameters()[0];
        var input = GetValue<T1>(parameter);
        var result = func(input);

        if (string.IsNullOrEmpty(key))
            key = func.Method.Name;
        TrackResult(key, await result);
        return this;
    }

    public FluentExecutor Then<T1, T2>(Action<T1, T2> action, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        var parameters = action.Method.GetParameters();
        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        try
        {
            action(input1, input2);
        }
        catch (Exception e)
        {
            if (string.IsNullOrEmpty(key))
                key = action.Method.Name;
            TrackResult(key, StepResult.Fail(new Error($"Action failed: {e.Message}")));
        }
        return this;
    }

    public async Task<FluentExecutor> Then<T1, T2>(Func<T1, T2, Task<IResult>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        var parameters = func.Method.GetParameters();
        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var result = await func(input1, input2);
        if (string.IsNullOrEmpty(key))
            key = func.Method.Name;
        TrackResult(key, result);
        return this;
    }

    public FluentExecutor Then<T1, T2, T3>(Action<T1, T2, T3> action, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        var parameters = action.Method.GetParameters();
        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var input3 = GetValue<T3>(parameters[2]);
        try
        {
            action(input1, input2, input3);
        }
        catch (Exception e)
        {
            if (string.IsNullOrEmpty(key))
                key = action.Method.Name;
            TrackResult(key, StepResult.Fail(new Error($"Action failed: {e.Message}")));
        }
        return this;
    }

    public async Task<FluentExecutor> Then<T1, T2, T3>(Func<T1, T2, T3, Task<IResult>> func, string? key = null)
    {
        _events?.OnStepStart?.Invoke("Action");
        _result.Metadata.Steps++;
        var parameters = func.Method.GetParameters();
        var input1 = GetValue<T1>(parameters[0]);
        var input2 = GetValue<T2>(parameters[1]);
        var input3 = GetValue<T3>(parameters[2]);
        var result = await func(input1, input2, input3);
        if (string.IsNullOrEmpty(key))
            key = func.Method.Name;
        TrackResult(key, result);
        return this;
    }

    public IResult<T> Execute<T>()
    {
        return _context.Get<T>();
    }
    
    public bool HasFailure => _failures.Count > 0;

    private T GetValue<T>(ParameterInfo parameter)
    {
        var key = GeyKey(parameter);
        return string.IsNullOrEmpty(key) ? _context.Get<T>().ValueOrDefault : _context.GetKeyed<T>(key).ValueOrDefault;
    }

    private string? GeyKey(ParameterInfo parameter)
    {

        var attribute = parameter?.GetCustomAttribute<KeyedByParameterNameAttribute>();

        string? key = null;

        if (attribute != null)
        {
            key = parameter.Name;
        }

        if (string.IsNullOrEmpty(key))
        {
            key = parameter?
                .GetCustomAttribute<KeyedByValueAttribute>()?.Key;
        }

        return key;
    }
    
    private void TrackResult<T>(string? key, IResult<T> result, string funcName)
    {
        if(string.IsNullOrEmpty(key))
            _context.Set(result);
        else _context.SetKeyed(key, result);

        _result.Reasons.AddRange(result.Reasons);

        if (result.IsFailed)
        {
            _failures.Add(new StepFailure(funcName, result.Errors));

            _result.Metadata.FirstFailure ??= funcName;

            _events?.OnStepFailure?.Invoke(funcName, result.Errors);
        }
        else
        {
            _events?.OnStepSuccess?.Invoke(funcName);
        }
    }

    private void TrackResult(string key, IResult result)
    {
        _result.Reasons.AddRange(result.Reasons);
    
        if (result.IsFailed)
        {
            _failures.Add(new StepFailure(key, result.Errors));
    
            _result.Metadata.FirstFailure ??= key;
    
            _events?.OnStepFailure?.Invoke(key, result.Errors);
        }
        else
        {
            _events?.OnStepSuccess?.Invoke(key);
        }
    }

    public FluentExecutor WithEvents(FluentExecutorEvents events)
    {
        _events = events;
        return this;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class KeyedByParameterNameAttribute : Attribute
{
    
}

[AttributeUsage(AttributeTargets.Parameter)]
public class KeyedByValueAttribute : Attribute
{
    public string Key { get; }
    public KeyedByValueAttribute(string key)
    {
        Key = key;
    }
}


public class FluentExecutorEvents
{
    public Action<string>? OnStepStart { get; set; }
    public Action<string>? OnStepSuccess { get; set; }
    public Action<string, IReadOnlyList<IError>>? OnStepFailure { get; set; }
}