namespace AzureFunctionsProject.Exceptions
{
    public class AccessorClientException : Exception
    {
        public AccessorClientException(string? message) : base(message)
        {
        }

        public AccessorClientException(string message, Exception inner)
            : base(message, inner) { }
    }

}
