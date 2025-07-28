using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models

// how the data is stored in the DB
{
    [Table("Tasks")]
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Payload { get; set; } = string.Empty;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
