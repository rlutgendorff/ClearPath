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
                .Then(CreateTest)
                .Then(CreateTest2);
        }

        [ReturnTypeKey("test")]
        public Task<IResult<Test>> CreateTest(Guid userId)
        {
            return Task.FromResult<IResult<Test>>(Result.Ok(new Test { Name = userId.ToString() }));
        }

        [ReturnTypeKey("result")]
        public Task<IResult<string>> CreateTest2(Test test)
        {
            return Task.FromResult<IResult<string>>(Result.Ok(test.Name));
        }
    }

    public class Test
    {
        public string Name { get; set; }
    }
}
