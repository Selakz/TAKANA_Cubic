#nullable enable

using T3Framework.Runtime.Event;

namespace MusicGame.Gameplay.Speed
{
	public class SpeedDataContainer : NotifiableDataContainer<ISpeed>
	{
		public override ISpeed InitialValue => new T3Speed(1.0f);

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