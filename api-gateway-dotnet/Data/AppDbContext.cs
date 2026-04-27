using Microsoft.EntityFrameworkCore;
using SdiApiGateway.Models.Entities;
using SdiApiGateway.Models.Enums;

namespace SdiApiGateway.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<InterviewRound> InterviewRounds => Set<InterviewRound>();
    public DbSet<EvaluationResult> EvaluationResults => Set<EvaluationResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store enums as strings in PostgreSQL
        modelBuilder.Entity<InterviewSession>(entity =>
        {
            entity.Property(e => e.CompanyMode)
                .HasConversion<string>();
            entity.Property(e => e.CurrentDifficulty)
                .HasConversion<string>();
            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.HasMany(e => e.Rounds)
                .WithOne(r => r.Session)
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InterviewRound>(entity =>
        {
            entity.Property(e => e.Difficulty)
                .HasConversion<string>();
        });
    }
}
