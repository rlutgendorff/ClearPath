using ClearPath.Results;

namespace ClearPath.DelegateExecutor.Tests
{
    public class DelegateExecutorTests
    {
        [Fact]
        public async Task Test1()
        {
            var userId = Guid.NewGuid();

            var result = await DelegateExecutor.StartWith("userId", userId)
                .Then("User", CreateTest);
        }

        public Task<IResult<Test>> CreateTest(Guid userId)
        {
            return Task.FromResult<IResult<Test>>(Result.Ok(new Test { Name = userId.ToString() }));
        }
    }

    public class Test
    {
        public string Name { get; set; }
    }
}
