namespace AzureFunctionsProject.Models
{
    /// <summary>
    /// Data transfer object for generic Data entities.
    /// Holds the unique identifier, payload JSON, and version for concurrency.
    /// </summary>
    public class DataDto
    {
        /// <summary>
        /// Unique identifier for the Data record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// JSON payload being stored or transferred.
        /// </summary>
        public string Payload { get; set; } = default!;

        /// <summary>
        /// PostgreSQL xmin version used as an ETag for optimistic concurrency.
        /// </summary>
        public uint Version { get; set; }
    }
}
