namespace ClearPath.Slim;

public interface IResult<out T>
{
    bool IsSuccess { get; }
    T Value { get; }
    string Error { get; }
}

public class Result<T> : IResult<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    protected Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value)
        => new Result<T>(true, value, null);

    public static Result<T> Fail(string error)
        => new Result<T>(false, default, error);
}