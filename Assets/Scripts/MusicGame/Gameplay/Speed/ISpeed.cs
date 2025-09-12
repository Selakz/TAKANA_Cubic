namespace MusicGame.Gameplay.Speed
{
	public interface ISpeed
	{
		/// <summary> The value to display </summary>
		public float Speed { get; set; }

		/// <summary> The actual logical speed </summary>
		public float SpeedRate { get; }
	}
}