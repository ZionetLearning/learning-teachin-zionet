using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models

// how the data is stored in the DB
{
    [Table("Tasks")]
    public record TaskModel
    {
        [Key]
        public int Id { get; init; }

        [Required]
        public string Name { get; init; } = string.Empty;

        [Required]
        public string Payload { get; init; } = string.Empty;

    }

}
