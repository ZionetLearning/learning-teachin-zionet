namespace Accessor.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, Exception inner) : base(message, inner) { }
    }

    public class NonRetryableException : Exception
    {
        public NonRetryableException(string message) : base(message) { }
        public NonRetryableException(string message, Exception inner) : base(message, inner) { }
    }

    public class RetryableException : Exception
    {
        public RetryableException(string message) : base(message) { }
        public RetryableException(string message, Exception inner) : base(message, inner) { }
    }
}
