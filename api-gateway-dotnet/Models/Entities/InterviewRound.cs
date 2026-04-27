using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SdiApiGateway.Models.Enums;

namespace SdiApiGateway.Models.Entities;

[Table("interview_rounds")]
public class InterviewRound
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("round_number")]
    public int RoundNumber { get; set; }

    [Required]
    [Column("question")]
    public string Question { get; set; } = string.Empty;

    [Column("user_answer")]
    public string? UserAnswer { get; set; }

    [Column("evaluation")]
    public string? Evaluation { get; set; }

    [Column("score")]
    public double? Score { get; set; }

    [Column("difficulty")]
    public DifficultyLevel? Difficulty { get; set; }

    [Column("topic_area")]
    public string? TopicArea { get; set; }

    [Column("answered_at")]
    public DateTime? AnsweredAt { get; set; }

    [Required]
    [Column("session_id")]
    public Guid SessionId { get; set; }

    [ForeignKey("SessionId")]
    public InterviewSession Session { get; set; } = null!;
}
