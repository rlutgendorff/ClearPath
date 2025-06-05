using ClearPath.Results;
using ClearPath.Reasons;
using Xunit;

namespace ClearPath.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Ok_Should_Be_Success()
    {
        var result = Result.Ok();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailed);
    }

    [Fact]
    public void Fail_Should_Be_Failed()
    {
        var result = Result.Fail(new Error("fail"));
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
    }
}
