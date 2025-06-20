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

        [Fact]
        public async Task Test2()
        {
            var userId = Guid.NewGuid();
            await DelegateExecutor.StartWith("userId", userId)
                .Then(CreateTest)
                .Then(CreateTest2)
                .Then(CreateTest3);
        }

        [ReturnTypeKey("test")]
        private Task<Result<Test>> CreateTest(Guid userId)
        {
            return Task.FromResult(Result.Ok(new Test { Name = userId.ToString() }));
        }

        [ReturnTypeKey("result")]
        private Task<Result<string>> CreateTest2(Test test)
        {
            return Task.FromResult(Result.Ok(test.Name));
        }

        private void CreateTest3(string result)
        {
            Console.WriteLine($"Result: {result}");
        }
    }

    public class Test
    {
        public string Name { get; set; }
    }
}
