using ClearPath.Results;
using System.Threading.Tasks;

namespace ClearPath.DelegateExecutor.Tests;

public class DelegateExecutorContextTests
{
    [Fact]
    public async Task Set_StoreValue()
    {
        var context = new DelegateExecutorContext();
        context.Set("testTask", "testValue", typeof(string));

        var task = context.Get("testTask", typeof(string));

        Assert.NotNull(task);
        Assert.IsType<Task<Result>>(task);

        var result = await task;
        
        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreGenericResultTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult(Result.Ok("Test"));
        context.Set("testTask", task, typeof(Task<Result<string>>));
        
        var response = context.Get("testTask", typeof(string));

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreResultTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult(Result.Ok());
        context.Set("testTask", task, typeof(Task<Result>));

        var response = context.Get("testTask");

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreValueTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult("Test");
        context.Set("testTask", task, typeof(Task<string>));

        var response = context.Get("testTask", typeof(string));

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreNullValue()
    {
        var context = new DelegateExecutorContext();
        string? task = null;
        context.Set("testTask", task, typeof(string));

        var response = context.Get("testTask", typeof(string));

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreNullResult()
    {
        var context = new DelegateExecutorContext();
        var task = Result.Ok<string?>(null);
        context.Set("testTask", task, typeof(Result<string>));

        var response = context.Get("testTask", typeof(string));

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreNullTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult<string?>(null);
        context.Set("testTask", task, typeof(Task<string>));

        var response = context.Get("testTask", typeof(string));

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreTask()
    {
        var context = new DelegateExecutorContext();
        var task = new Task(() => Console.Write("test"));
        context.Set("testTask", task, typeof(Task));

        var response = context.Get("testTask");

        Assert.NotNull(task);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreVoidTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.CompletedTask;
        context.Set("testTask", task, typeof(Task));

        var response = context.Get("testTask");

        Assert.NotNull(task);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreGenericResult()
    {
        var context = new DelegateExecutorContext();
        var task = Result.Ok("testTask");
        context.Set("testTask", task, typeof(Result<string>));

        var response = context.Get("testTask", typeof(string));

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result<string>);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }

    [Fact]
    public async Task Set_StoreResult()
    {
        var context = new DelegateExecutorContext();
        var task = Result.Ok();
        context.Set("testTask", task, typeof(Result));

        var response = context.Get("testTask");

        Assert.NotNull(response);
        Assert.IsType<Task<Result>>(response);

        var result = await response;

        var resultType = result.GetType();
        var expectedType = typeof(Result);

        Assert.True(expectedType.IsAssignableFrom(resultType),
            $"Expected assignable to Result<string>, but got {resultType}");
    }
}