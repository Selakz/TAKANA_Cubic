using MusicGame.Components.Notes;

public class InputInfo
{
	public INote Note { get; set; }

	/// <summary> 输入时间默认为正无穷 </summary>
	public float TimeInput { get; set; } = float.PositiveInfinity;
}