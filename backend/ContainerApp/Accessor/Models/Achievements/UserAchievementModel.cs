using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models.Achievements;

[Table("UserAchievements")]
public class UserAchievementModel
{
    [Key]
    public Guid UserAchievementId { get; set; }

    public required Guid UserId { get; set; }
    public required Guid AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }
}
