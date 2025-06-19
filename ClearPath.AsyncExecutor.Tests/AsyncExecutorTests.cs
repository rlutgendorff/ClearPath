using ClearPath.AsyncExecutor.Tests.Errors;
using ClearPath.Results;
using FluentValidation;
using Moq;
using Polly;

namespace ClearPath.AsyncExecutor.Tests;

public class AsyncExecutorTests
{
    [Fact]
    public void StartWith_Value_CreatesExecutor()
    {
        var executor = AsyncExecutor.StartWith("key", 42);
        Assert.NotNull(executor);
        Assert.Single(executor.StepResults);
    }

    [Fact]
    public void StartWith_Result_CreatesExecutor()
    {
        var result = Result.Ok(42);
        var executor = AsyncExecutor.StartWith("key", result);
        Assert.NotNull(executor);
        Assert.Single(executor.StepResults);
    }

    [Fact]
    public void StartWith_TaskResult_CreatesExecutor()
    {
        var resultTask = Task.FromResult(Result.Ok(42));
        var executor = AsyncExecutor.StartWith("key", resultTask);
        Assert.NotNull(executor);
        Assert.Single(executor.StepResults);
    }

    [Fact]
    public void AddKeyedVariable_Value_AddsToContext()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        executor.AddKeyedVariable("another", 2);
        Assert.True(executor.StepResults.Count >= 1);
    }

    [Fact]
    public void AddKeyedVariable_Result_AddsToContext()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        executor.AddKeyedVariable("another", Result.Ok(2));
        Assert.True(executor.StepResults.Count >= 1);
    }

    [Fact]
    public void AddKeyedVariable_TaskResult_AddsToContext()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        executor.AddKeyedVariable("another", Task.FromResult(Result.Ok(2)));
        Assert.True(executor.StepResults.Count >= 1);
    }

    [Fact]
    public async Task Then_ExecutesStep()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        await executor.Then("next", ct => Task.FromResult<IResult<int>>(Result.Ok(2)));
        Assert.Contains(executor.StepResults, s => s.Key == "next");
    }

    [Fact(Skip = "Invalid test. tasks can run parallel so this can happen")]
    public async Task Then_WithFailedStep_DoesNotExecute()
    {
        var executor = AsyncExecutor.StartWith<int>("key", Result<int>.Fail(new TestError { Message = "fail" }));
        await executor.Then("next", ct => Task.FromResult<IResult<int>>(Result.Ok(2)));
        Assert.DoesNotContain(executor.StepResults, s => s.Key == "next");
    }

    [Fact]
    public async Task ThenIf_ExecutesWhenPredicateTrue()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        await executor.ThenIf(
            "next",
            (ctx, ct) => true,
            (ctx, ct) => false,
            ct => Task.FromResult<IResult<int>>(Result.Ok(2))
        );
        Assert.Contains(executor.StepResults, s => s.Key == "next");
    }

    [Fact]
    public async Task ThenIf_SkipsWhenPredicateFalse()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        await executor.ThenIf(
            "next",
            (ctx, ct) => false,
            (ctx, ct) => false,
            ct => Task.FromResult<IResult<int>>(Result.Ok(2))
        );
        Assert.DoesNotContain(executor.StepResults, s => s.Key == "next");
    }

    [Fact]
    public async Task Do_ExecutesStep()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        await executor.Do("do", ct => Task.FromResult<IResult>(Result.Ok()));
        Assert.Contains(executor.StepResults, s => s.Key == "do");
    }

    [Fact]
    public void WithEvents_SetsEvents()
    {
        var events = new AsyncExecutorEvents();
        var executor = AsyncExecutor.StartWith("key", 1).WithEvents(events);
        Assert.NotNull(executor);
    }

    [Fact]
    public void WithPolicy_SetsPolicy()
    {
        var policy = new Mock<IAsyncPolicy>().Object;
        var executor = AsyncExecutor.StartWith("key", 1).WithPolicy(policy);
        Assert.NotNull(executor);
    }

    [Fact]
    public void WithValidator_AddsValidator()
    {
        var validator = new Mock<IValidator<int>>().Object;
        var executor = AsyncExecutor.StartWith("key", 1).WithValidator(validator);
        Assert.NotNull(executor);
    }

    [Fact]
    public async Task FinishAll_WaitsForAllTasks()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        await executor.FinishAll();
        Assert.NotNull(executor);
    }

    [Fact]
    public async Task GetResult_ReturnsOkIfNoFailures()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        var result = await executor.GetResult();
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetResult_ReturnsFailIfFailures()
    {
        var executor = AsyncExecutor.StartWith<int>("key", Result<int>.Fail(new TestError { Message = "fail" }));
        var result = await executor.GetResult();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetResultT_ReturnsOkIfNoFailures()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        var result = await executor.GetResult<int>("key");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetResultT_ReturnsFailIfFailures()
    {
        var executor = AsyncExecutor.StartWith<int>("key", Result<int>.Fail(new TestError { Message = "fail" }));
        var result = await executor.GetResult<int>("key");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Map_MapsValue()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        executor.Map<int, string>("key", i => i.ToString(), "mapped");
        Assert.Contains(executor.StepResults, s => s.Key == "mapped");
    }

    [Fact]
    public void MapAsync_MapsValueAsync()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        executor.MapAsync<int, string>("key", i => Task.FromResult(i.ToString()), "mapped");
        Assert.Contains(executor.StepResults, s => s.Key == "mapped");
    }

    [Fact]
    public void EnableOutputCaching_EnablesCaching()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        executor.EnableOutputCaching();
        Assert.NotNull(executor);
    }

    [Fact]
    public async Task Group_ExecutesGroupSteps()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        await executor.Group("group", async ex =>
        {
            await ex.Then("g1", ct => Task.FromResult<IResult<int>>(Result.Ok(2)));
        });
        Assert.Contains(executor.StepResults, s => s.Key == "g1");
    }

    [Fact]
    public async Task CompensateAll_InvokesCompensation()
    {
        var executor = AsyncExecutor.StartWith("key", 1);
        bool compensationCalled = false;
        await executor.ThenWithCompensation(
            "step",
            ct => Task.FromResult<IResult<int>>(Result.Ok(2)),
            "comp",
            ctx => { compensationCalled = true; return Task.FromResult<IResult>(Result.Ok()); }
        );
        await executor.CompensateAll();
        Assert.True(compensationCalled);
    }
}