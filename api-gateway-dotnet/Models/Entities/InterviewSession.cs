using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SdiApiGateway.Models.Enums;

namespace SdiApiGateway.Models.Entities;

[Table("interview_sessions")]
public class InterviewSession
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("topic")]
    public string Topic { get; set; } = string.Empty;

    [Required]
    [Column("company_mode")]
    public CompanyMode CompanyMode { get; set; }

    [Required]
    [Column("current_difficulty")]
    public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.MEDIUM;

    [Required]
    [Column("current_round")]
    public int CurrentRound { get; set; }

    [Required]
    [Column("max_rounds")]
    public int MaxRounds { get; set; }

    [Required]
    [Column("status")]
    public SessionStatus Status { get; set; } = SessionStatus.IN_PROGRESS;

    [Required]
    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    public List<InterviewRound> Rounds { get; set; } = new();

    public void AddRound(InterviewRound round)
    {
        round.SessionId = Id;
        round.Session = this;
        Rounds.Add(round);
    }
}
