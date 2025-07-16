namespace TodoManager.Models;

public class DaprBindingRequest
{
    public Todo? Data { get; set; }
    public string? Operation { get; set; }
}
