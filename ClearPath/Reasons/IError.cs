namespace ClearPath.Reasons;

public interface IError : IReason
{
   List<IError> Reasons { get; }
}

 public class Error : IError
    {
        public string Message { get; protected set; }
        public Dictionary<string, object> Metadata { get; }
        public List<IError> Reasons { get; }
        
        protected Error()
        {
            Metadata = new Dictionary<string, object>();
            Reasons = new List<IError>();
        }
        
        public Error(string message)
            : this()
        {
            Message = message;
        }

        public Error(string message, IError causedBy)
            : this(message)
        {
            if (causedBy == null)
                throw new ArgumentNullException(nameof(causedBy));

            Reasons.Add(causedBy);
        }

        // public Error CausedBy(IError error)
        // {
        //     if (error == null)
        //         throw new ArgumentNullException(nameof(error));
        //
        //     Reasons.Add(error);
        //     return this;
        // }
        //
        // public Error CausedBy(Exception exception)
        // {
        //     if (exception == null)
        //         throw new ArgumentNullException(nameof(exception));
        //
        //     Reasons.Add(Result.Settings.ExceptionalErrorFactory(null, exception));
        //     return this;
        // }
        //
        // public Error CausedBy(string message, Exception exception)
        // {
        //     if (exception == null)
        //         throw new ArgumentNullException(nameof(exception));
        //
        //     Reasons.Add(Result.Settings.ExceptionalErrorFactory(message, exception));
        //     return this;
        // }
        //
        // public Error CausedBy(string message)
        // {
        //     Reasons.Add(Result.Settings.ErrorFactory(message));
        //     return this;
        // }
        //
        // public Error CausedBy(IEnumerable<IError> errors)
        // {
        //     if (errors == null)
        //         throw new ArgumentNullException(nameof(errors));
        //
        //     Reasons.AddRange(errors);
        //     return this;
        // }
        //
        // public Error CausedBy(IEnumerable<string> errors)
        // {
        //     if (errors == null)
        //         throw new ArgumentNullException(nameof(errors));
        //
        //     Reasons.AddRange(errors.Select(errorMessage => Result.Settings.ErrorFactory(errorMessage)));
        //     return this;
        // }
        //
        // public Error WithMetadata(string metadataName, object metadataValue)
        // {
        //     Metadata.Add(metadataName, metadataValue);
        //     return this;
        // }
        //
        // public Error WithMetadata(Dictionary<string, object> metadata)
        // {
        //     foreach (var metadataItem in metadata)
        //     {
        //         Metadata.Add(metadataItem.Key, metadataItem.Value);
        //     }
        //
        //     return this;
        // }
        //
        // public override string ToString()
        // {
        //     return new ReasonStringBuilder()
        //         .WithReasonType(GetType())
        //         .WithInfo(nameof(Message), Message)
        //         .WithInfo(nameof(Metadata), string.Join("; ", Metadata))
        //         .WithInfo(nameof(Reasons), ReasonFormat.ErrorReasonsToString(Reasons))
        //         .Build();
        // }
    }