namespace Common.Callbacks;

public interface ICallbackContextManager
{
    IDictionary<string, string> ToHeaders(CallbackContext context);
    CallbackContext FromHeaders(IDictionary<string, string> headers);
}