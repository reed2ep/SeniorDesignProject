using System.Drawing;

public class Hold
{
    public Rectangle Bounds { get; set; }  // Position and size of the hold
    public HoldDifficulty Difficulty { get; set; }  // Difficulty level
}

public enum HoldDifficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3,
    Extreme = 4 
}
