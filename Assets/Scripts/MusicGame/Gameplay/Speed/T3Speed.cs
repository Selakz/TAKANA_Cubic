using UnityEngine;

namespace MusicGame.Gameplay.Speed
{
	public struct T3Speed : ISpeed
	{
		private float speed;

		public float Speed
		{
			get => speed;
			set
			{
				value = Mathf.Clamp(value, 1.0f, 10.0f);
				speed = value;
			}
		}

		// Magic formula
		public float SpeedRate => 2.8125f / (Mathf.Sin((0.85f + 0.056f * Speed) * Mathf.PI) + 1f);

		public T3Speed(float speed)
		{
			this.speed = 0;
			Speed = speed;
		}
	}
}