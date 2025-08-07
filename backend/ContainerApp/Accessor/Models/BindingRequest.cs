namespace Accessor.Models;

public class BindingRequest<T>
{
    public T Data { get; set; } = default!;
    public Dictionary<string, string>? Metadata { get; set; }
}
