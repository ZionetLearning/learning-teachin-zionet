namespace AzureFunctionsProject.Exceptions
{
    public class EngineClientException : Exception
    {
        public EngineClientException(string msg, Exception? inner = null)
            : base(msg, inner) { }
    }

}
