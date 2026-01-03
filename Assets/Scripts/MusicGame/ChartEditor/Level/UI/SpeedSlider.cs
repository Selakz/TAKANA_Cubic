#nullable enable

using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SpeedSlider : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Slider speedSlider = default!;
		[SerializeField] private TMP_Text speedText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ISpeed>(speed, (_, _) =>
			{
				speedSlider.SetValueWithoutNotify(speed.Value.Speed * 10);
				speedText.text = speed.Value.Speed.ToString("0.0");
			}),
			new SliderRegistrar(speedSlider, value =>
			{
				speed.Value.Speed = value / 10;
				speed.ForceNotify();
			})
		};

		// Private
		private NotifiableProperty<ISpeed> speed = default!;

		// Defined Functions
		[Inject]
		private void Construct(NotifiableProperty<ISpeed> speed)
		{
			this.speed = speed;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}