using ClearPath.Results;

namespace ClearPath.DelegateExecutor.Tests;

public class DelegateExecutorContextTests
{
    [Fact]
    public void Set_StoreValue()
    {
        var context = new DelegateExecutorContext();
        context.Set("test", "testValue");
    }

    [Fact]
    public void Set_StoreGenericResultTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult(Result.Ok("Test"));
        context.Set("testTask", task);
    }

    [Fact]
    public void Set_StoreResultTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult(Result.Ok());
        context.Set("testTask", task);
    }

    [Fact]
    public void Set_StoreValueTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.FromResult("Test");
        context.Set("testTask", task);
    }

    [Fact]
    public void Set_StoreTask()
    {
        var context = new DelegateExecutorContext();
        var task = Task.CompletedTask;
        context.Set("testTask", task);
    }

    [Fact]
    public void Set_StoreGenericResult()
    {
        var context = new DelegateExecutorContext();
        var result = Result.Ok("TestResult");
        context.Set("testResult", result);
    }

    [Fact]
    public void Set_StoreResult()
    {
        var context = new DelegateExecutorContext();
        var result = Result.Ok();
        context.Set("testResultInt", result);
    }
}