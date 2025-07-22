namespace AzureFunctionsProject.Models
{
    public class QueueMessage
    {
        public string Action { get; set; } = default!;
        public DataDto? Entity { get; set; }
        public Guid? Id { get; set; }
        public uint? Version { get; set; }
    }
}
