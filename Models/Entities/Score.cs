using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScoreTracker.Api.Models.Entities;

public class Score
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Value { get; set; }

    [Required]
    public DateTime DateRecorded { get; set; }

    // Foreign Key to the ApplicationUser
    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
