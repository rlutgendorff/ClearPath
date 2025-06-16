namespace ClearPath.AsyncExecutor;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class KeyedByAttribute : Attribute
{
    public string Key { get; }
    public KeyedByAttribute(string key) => Key = key;
}