using ClearPath.Builders;
using ClearPath.Extensions;
using ClearPath.Reasons;
using ClearPath.Results;

namespace ClearPath.Tests.Builders;

public class ResultBuilderTests
{
    [Fact]
    public void StartWith_WithFunc_ReturnsOkResult()
    {
        ResultBuilder.StartWith("Test", () => Result.Ok("test"));
    }
    
    [Fact]
    public void StartWith_WithValue_ReturnsOkResult()
    {
        ResultBuilder.StartWith("Test", Result.Ok("test"));
    }

    [Fact]
    public void With_AddsValueToBuilder()
    {
        var result = ResultBuilder.StartWith("Test", () => Result.Ok("test"))
            .With("Test2", true)
            .Get<bool>("Test2");
        
        Assert.True(result.Value);
    }
    
    [Fact]
    public void WithEvents_EventsAreTriggered_WhenSucceeded()
    {
        var stepFailureTriggered = false;
        var stepStartTriggered = false;
        var stepSuccessTriggered = false;

        var events = new ResultBuilderEvents
        {
            OnStepFailure = (_, _) => stepFailureTriggered = true,
            OnStepStart = _ => stepStartTriggered = true,
            OnStepSuccess = _ => stepSuccessTriggered = true
        };
        
        ResultBuilder.StartWith("Test", () => Result.Ok("test"))
            .WithEvents(events)
            .DoWhenSuccess("Test2", _ => Result.Ok(true))
            .Get<bool>("Test2");
        
        Assert.False(stepFailureTriggered);
        Assert.True(stepStartTriggered);
        Assert.True(stepSuccessTriggered);
    }
    
    [Fact]
    public void WithEvents_EventsAreTriggered_WhenFailed()
    {
        var stepFailureTriggered = false;
        var stepStartTriggered = false;
        var stepSuccessTriggered = false;

        var events = new ResultBuilderEvents
        {
            OnStepFailure = (_, _) => stepFailureTriggered = true,
            OnStepStart = _ => stepStartTriggered = true,
            OnStepSuccess = _ => stepSuccessTriggered = true
        };
        
        ResultBuilder.StartWith("Test", () => Result.Ok("test"))
            .WithEvents(events)
            .DoWhenSuccess("Test2", _ => Result<object>.Fail(new Error("Test")))
            .Get<object>("Test2");
        
        Assert.True(stepFailureTriggered);
        Assert.True(stepStartTriggered);
        Assert.False(stepSuccessTriggered);
    }
    
    [Fact]
    public void ResultBuilderContext_Get_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(() => ResultBuilder.StartWith("Test", () => Result.Ok("test"))
            .DoWhenSuccess("Test2", context => context.Get<object>("notExisting")));
    }
    
    [Fact]
    public void ResultBuilderContext_TryGet_WhenFailed_ResultIsFalse()
    {
        var returnResult = true;
        IResult<object>? outputResult = null;
        
        ResultBuilder.StartWith("Test", () => Result.Ok("test"))
            .DoWhenSuccess("Test2", context =>
            {
                returnResult = context.TryGet<object>("notExisting", out var output);
                outputResult = output;
                
                return Result.Ok(new object());
            });
        
        Assert.False(returnResult);
        Assert.Null(outputResult);
    }
    
    [Fact]
    public async Task DoWhenSuccessAsync_WhenValidFunc()
    {
        await ResultBuilder.StartWith("Test", () => Result.Ok("test"))
            .DoWhenSuccessAsync("Test2", () => Task.FromResult<IResult<string>>(Result.Ok("test")));
    }
    
    [Fact]
    public void DoWhenSuccess_WhenHasFailure_SkipsExecution()
    {
        var executed = false;
        var builder = ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("Initial failure")))
            .DoWhenSuccess("Next", _ => 
            {
                executed = true;
                return Result.Ok(true);
            });

        Assert.False(executed);
        Assert.True(builder.HasFailure);
    }

    [Fact]
    public void OnSuccess_WhenSpecificKeySucceeds_ExecutesAction()
    {
        var executed = false;
        ResultBuilder
            .StartWith("Test", Result.Ok("success"))
            .OnSuccess("Test", _ => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void OnSuccess_WhenSpecificKeyFails_DoesNotExecuteAction()
    {
        var executed = false;
        ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("failure")))
            .OnSuccess("Test", _ => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void OnFailure_WhenHasFailures_ExecutesAction()
    {
        var capturedFailures = new List<StepFailure>();
        ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("failure")))
            .OnFailure((_, failures) => capturedFailures = failures.ToList());

        Assert.Single(capturedFailures);
        Assert.Equal("Test", capturedFailures[0].Key);
    }

    [Fact]
    public void OnFailure_WithFallback_ExecutesFallbackForSpecificKey()
    {
        var builder = ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("failure")))
            .OnFailure("Test", _ => Result.Ok("fallback"), "FallbackKey");

        var result = builder.Get<string>("FallbackKey");
        Assert.True(result.IsSuccess);
        Assert.Equal("fallback", result.Value);
    }

    [Fact]
    public void RetryOnFailure_WhenSucceedsOnRetry_UpdatesResult()
    {
        var attempts = 0;
        var builder = ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("initial failure")))
            .RetryOnFailure("Test", _ =>
            {
                attempts++;
                return attempts == 2 ? Result.Ok("success") : Result<string>.Fail(new Error("retry failure"));
            }, maxAttempts: 3);

        var result = builder.Get<string>("Test");
        Assert.True(result.IsSuccess);
        Assert.Equal("success", result.Value);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task RetryOnFailureAsync_WhenExhaustsAttempts_MaintainsFailure()
    {
        var attempts = 0;
        var builder = await ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("initial failure")))
            .RetryOnFailureAsync("Test", _ =>
            {
                attempts++;
                return Task.FromResult<IResult>(Result<string>.Fail(new Error("persistent failure")));
            }, maxAttempts: 2);

        Assert.True(builder.HasFailure);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public void BuildOnSuccess_WhenHasFailure_ReturnsNull()
    {
        var result = ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("failure")))
            .BuildOnSuccess<string>("Test");

        Assert.Null(result);
    }

    [Fact]
    public void GetFailures_ReturnsAllFailures()
    {
        var builder = ResultBuilder
            .StartWith("Test1", Result<string>.Fail(new Error("failure1")))
            .DoWhenSuccess("Test2", _ => Result<string>.Fail(new Error("failure2")));

        var failures = builder.GetFailures();
        Assert.Single(failures);  // Should only have one since DoWhenSuccess was skipped
        Assert.Equal("Test1", failures[0].Key);
    }

    [Fact]
    public async Task DoWhenSuccessAsync_WithMultipleArguments_ExecutesSuccessfully()
    {
        var builder = await ResultBuilder
            .StartWith("Initial", Result.Ok("start"))
            .DoWhenSuccessAsync<string, string, int, bool>(
                "MultiArg",
                (s, i, b) => Task.FromResult<IResult<string>>(Result.Ok($"{s}{i}{b}")),
                _ => "test",
                _ => 42,
                _ => true
            );

        var result = builder.Get<string>("MultiArg");
        Assert.True(result.IsSuccess);
        Assert.Equal("test42True", result.Value);
    }
    
    [Fact]
    public async Task DoWhenSuccessAsync_WithMultipleArguments_ExecutesFailed()
    {
        var builder = await ResultBuilder
            .StartWith("Initial", Result.Ok("start"))
            .DoWhenSuccess("Test", _ => Result<string>.Fail(new Error("failure")))
            .DoWhenSuccessAsync<string, string, int, bool>(
                "MultiArg",
                (s, i, b) => Task.FromResult<IResult<string>>(Result.Ok($"{s}{i}{b}")),
                _ => "test",
                _ => 42,
                _ => true
            );

        var result = builder.BuildOnSuccess<string>("MultiArg");
        Assert.Null(result);
    }
    
    [Fact]
    public void ResultBuilderContext_TryGet_WhenNonGeneric_ReturnsTrue()
    {
        var builder = ResultBuilder.StartWith("Test", Result.Ok("test"));
        builder.DoWhenSuccess("Check", context =>
        {
            Assert.True(context.TryGet("Test", out var result));
            Assert.NotNull(result);
            return Result.Ok(true);
        });
    }

    [Fact]
    public void ResultBuilderContext_TryGet_Generic_WithCorrectType_ReturnsTrue()
    {
        var builder = ResultBuilder.StartWith("Test", Result.Ok("test"));
        builder.DoWhenSuccess("Check", context =>
        {
            Assert.True(context.TryGet<string>("Test", out var result));
            Assert.NotNull(result);
            Assert.Equal("test", result.Value);
            return Result.Ok(true);
        });
    }

    [Fact]
    public void ResultBuilderContext_TryGet_Generic_WithWrongType_ReturnsFalse()
    {
        var builder = ResultBuilder.StartWith("Test", Result.Ok("test"));
        builder.DoWhenSuccess("Check", context =>
        {
            Assert.False(context.TryGet<int>("Test", out var result));
            Assert.Null(result);
            return Result.Ok(true);
        });
    }

    [Fact]
    public void Build_NonGeneric_ReturnsResult()
    {
        var builder = ResultBuilder.StartWith("Test", Result.Ok("test"));
        var result = builder.Get("Test");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DoWhenSuccessAsync_WithContext_ExecutesSuccessfully()
    {
        var builder = await ResultBuilder
            .StartWith("Test", Result.Ok("test"))
            .DoWhenSuccessAsync("Next", _ => Task.FromResult<IResult<string>>(Result.Ok("success")));
        
        var result = builder.Get<string>("Next");
        Assert.True(result.IsSuccess);
        Assert.Equal("success", result.Value);
    }
    
    [Fact]
    public async Task DoWhenSuccessAsync_WithContext_ExecutesFailed()
    {
        var builder = await ResultBuilder
            .StartWith("Test", Result.Ok("test"))
            .DoWhenSuccessAsync("Next", _ => Task.FromResult<IResult<string>>(Result<string>.Fail(new Error("failure"))))
            .DoWhenSuccessAsync("Next2", _ => Task.FromResult<IResult<string>>(Result.Ok("success")));
        
        var result = builder.Get<string>("Next");
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task DoWhenSuccessAsync_WithNoContext_ExecutesSuccessfully()
    {
        var builder = await ResultBuilder
            .StartWith("Test", Result.Ok("test"))
            .DoWhenSuccessAsync("Next", () => Task.FromResult<IResult<string>>(Result.Ok("success")));
        
        var result = builder.Get<string>("Next");
        Assert.True(result.IsSuccess);
        Assert.Equal("success", result.Value);
    }

    [Fact]
    public void OnSuccess_WithoutKey_ExecutesOnOverallSuccess()
    {
        var executed = false;
        ResultBuilder
            .StartWith("Test", Result.Ok("test"))
            .OnSuccess(_ => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void OnFailure_WithoutFailures_DoesNotExecuteAction()
    {
        var executed = false;
        ResultBuilder
            .StartWith("Test", Result.Ok("test"))
            .OnFailure((_, _) => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void OnFailure_WithFallback_WhenKeyDoesNotMatch_DoesNotExecuteFallback()
    {
        var builder = ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("failure")))
            .OnFailure("DifferentKey", _ => Result.Ok("fallback"), "FallbackKey");

        Assert.True(builder.HasFailure);
        Assert.Throws<KeyNotFoundException>(() => builder.Get<string>("FallbackKey"));
    }

    [Fact]
    public async Task RetryOnFailureAsync_WhenKeyDoesNotMatch_DoesNotRetry()
    {
        var attempts = 0;
        var builder = await ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("failure")))
            .RetryOnFailureAsync("DifferentKey", _ =>
            {
                attempts++;
                return Task.FromResult<IResult>(Result.Ok("success"));
            });

        Assert.True(builder.HasFailure);
        Assert.Equal(0, attempts);
    }

    [Fact]
    public void BuildOnSuccess_WhenNoFailure_ReturnsResult()
    {
        var result = ResultBuilder
            .StartWith("Test", Result.Ok("success"))
            .BuildOnSuccess<string>("Test");

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("success", result.Value);
    }

    [Fact]
    public async Task DoWhenSuccessAsync_WithTwoArguments_ExecutesSuccessfully()
    {
        var builder = await ResultBuilder
            .StartWith("Initial", Result.Ok("start"))
            .DoWhenSuccessAsync<string, string, int>(
                "TwoArgs",
                (s, i) => Task.FromResult<IResult<string>>(Result.Ok($"{s}{i}")),
                _ => "test",
                _ => 42
            );

        var result = builder.Get<string>("TwoArgs");
        Assert.True(result.IsSuccess);
        Assert.Equal("test42", result.Value);
    }

    [Fact]
    public async Task DoWhenSuccessAsync_WithOneArgument_ExecutesSuccessfully()
    {
        var builder = await ResultBuilder
            .StartWith("Initial", Result.Ok("start"))
            .DoWhenSuccessAsync<string, string>(
                "OneArg",
                s => Task.FromResult<IResult<string>>(Result.Ok($"processed-{s}")),
                _ => "test"
            );

        var result = builder.Get<string>("OneArg");
        Assert.True(result.IsSuccess);
        Assert.Equal("processed-test", result.Value);
    }

    [Fact]
    public void RetryOnFailure_WhenMaxAttemptsReached_MaintainsFailure()
    {
        var attempts = 0;
        var builder = ResultBuilder
            .StartWith("Test", Result<string>.Fail(new Error("initial failure")))
            .RetryOnFailure("Test", _ =>
            {
                attempts++;
                return Result<string>.Fail(new Error("persistent failure"));
            }, maxAttempts: 2);

        var result = builder.Get<string>("Test");
        Assert.True(result.IsFailed);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public void Build_SetsFirstFailure()
    {
        var builder = ResultBuilder
            .StartWith("Start", Result.Ok("init"))
            .DoWhenSuccess("FailStep", _ => Result<string>.Fail(new Error("failure")))
            .DoWhenSuccess("Skipped", _ => Result.Ok("skip"));

        var result = builder.Build(_ => "final");

        var stepResult = Assert.IsType<StepResult<string>>(result);
        Assert.Equal("FailStep", stepResult.Metadata.FirstFailure);
    }

    [Fact]
    public void Build_IncrementsStepCount()
    {
        var builder = ResultBuilder
            .StartWith("Start", Result.Ok("init"))
            .DoWhenSuccess("Step1", _ => Result.Ok(true))
            .DoWhenSuccess("Step2", _ => Result.Ok(true));

        var result = builder.Build(_ => "done");

        var stepResult = Assert.IsType<StepResult<string>>(result);
        Assert.Equal(2, stepResult.Metadata.Steps);
    }

}