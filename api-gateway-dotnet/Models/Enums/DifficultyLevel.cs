namespace SdiApiGateway.Models.Enums;

public enum DifficultyLevel
{
    BEGINNER = 1,
    EASY = 3,
    MEDIUM = 5,
    HARD = 7,
    EXPERT = 9
}

public static class DifficultyLevelExtensions
{
    /// <summary>
    /// Adjust difficulty based on the candidate's score.
    /// Strong answer (>= 8) → increase difficulty.
    /// Weak answer (<= 4) → decrease difficulty.
    /// Otherwise → maintain.
    /// </summary>
    public static DifficultyLevel Adjust(this DifficultyLevel current, double score)
    {
        if (score >= 8.0)
        {
            return current switch
            {
                DifficultyLevel.BEGINNER => DifficultyLevel.EASY,
                DifficultyLevel.EASY => DifficultyLevel.MEDIUM,
                DifficultyLevel.MEDIUM => DifficultyLevel.HARD,
                _ => DifficultyLevel.EXPERT
            };
        }

        if (score <= 4.0)
        {
            return current switch
            {
                DifficultyLevel.EXPERT => DifficultyLevel.HARD,
                DifficultyLevel.HARD => DifficultyLevel.MEDIUM,
                DifficultyLevel.MEDIUM => DifficultyLevel.EASY,
                _ => DifficultyLevel.BEGINNER
            };
        }

        return current;
    }
}
