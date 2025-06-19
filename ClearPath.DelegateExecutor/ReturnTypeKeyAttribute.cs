namespace ClearPath.DelegateExecutor;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ReturnTypeKeyAttribute : Attribute
{
    public string Key { get; }
    public ReturnTypeKeyAttribute(string key) => Key = key;
}