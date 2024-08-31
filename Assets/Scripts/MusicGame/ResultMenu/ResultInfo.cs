using Takana3.MusicGame.LevelSelect;
using System.Collections.Generic;

public class ResultInfo : IPassInfo
{
    public LevelInfo LevelInfo { get; set; }
    public Dictionary<(NoteType, JudgeResult), int> JudgeCount { get; set; }
    public Dictionary<NoteType, double> NoteScore { get; set; }
    public double Score { get; set; }
    public int MaxCombo { get; set; }
}
