#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Gameplay.Speed
{
	public class SpeedDataContainer : NotifiableDataContainer<ISpeed>
	{
		[SerializeField] private float speed = 1;

		public override ISpeed InitialValue => new T3Speed(speed);

		public float Speed
		{
			get => Property.Value.Speed;
			set
			{
				Property.Value.Speed = value;
				Property.ForceNotify();
			}
		}
	}
}