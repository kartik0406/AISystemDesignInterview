using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdiApiGateway.Models.Entities;

[Table("evaluation_results")]
public class EvaluationResult
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Required]
    [Column("overall_score")]
    public double OverallScore { get; set; }

    [Column("strengths")]
    public string? Strengths { get; set; }

    [Column("weaknesses")]
    public string? Weaknesses { get; set; }

    [Column("suggestions")]
    public string? Suggestions { get; set; }

    [Column("rubric_breakdown")]
    public string? RubricBreakdown { get; set; }

    [Column("architecture_diagram")]
    public string? ArchitectureDiagram { get; set; }

    [Required]
    [Column("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
