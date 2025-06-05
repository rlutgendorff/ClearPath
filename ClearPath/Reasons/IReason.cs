namespace ClearPath.Reasons;

public interface IReason
{
    /// <summary>
    /// The reason's message
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Any metadata added to the reason
    /// </summary>
    Dictionary<string, object> Metadata { get; }
}