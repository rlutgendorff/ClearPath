using ClearPath.Executors;
using ClearPath.Results;

namespace ClearPath.Tests.Executors;

public class FluentExecutorTests
{
    [Fact]
    public void FluentExecutor_Then_CallsMethodWithCorrectParameters()
    {
        // Arrange
        var initialResult = Result.Ok(5);
        var executor = FluentExecutor.StartWith(initialResult);
        
        // Act
        var result = executor
            .Then<int, string>(Test1)
            .Execute<string>();
        
        // Assert
        Assert.Equal("Hello world 5", result.Value);
    }

    [Fact]
    public void FluentExecutor_Then_WithKeyedParameter_CallsMethodWithCorrectParameters()
    {
        // Arrange
        var initialResult = Result.Ok(5);
        var executor = FluentExecutor
            .StartWith(initialResult)
            .AddKeyedVariable("specificName", Result.Ok(10));
        
        // Act
        var result = executor
            .Then<int, string>(Test2)
            .Execute<string>();
        
        // Assert
        Assert.Equal("Hello world 10", result.Value);
    }

    private IResult<string> Test1(int i)
    {
        return Result.Ok($"Hello world {i}");
    }

    private IResult<string> Test2([KeyedByParameterName]int specificName)
    {
        return Result.Ok($"Hello world {specificName}");
    }
}