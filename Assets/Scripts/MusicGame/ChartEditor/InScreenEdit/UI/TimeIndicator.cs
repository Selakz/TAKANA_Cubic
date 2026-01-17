#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class TimeIndicator : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private GameObject indicator = default!;
		[SerializeField] private SpriteRenderer lineTexture = default!;
		[SerializeField] private TMP_Text timeText = default!;

		public Modifier<Color> ColorModifier => colorModifier ??= new(
			() => lineTexture.color,
			color =>
			{
				lineTexture.color = color;
				timeText.color = color;
			});

		public Modifier<string> TextModifier => textModifier ??= new(
			() => timeText.text,
			text => timeText.text = text, "0");

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				indicatorActiveModifier.Register(_ => info is not null, 0);
			}),
			new PropertyRegistrar<bool>(ISingletonSetting<InScreenEditSetting>.Instance.ShowTimeIndicator, (_, _) =>
			{
				var shouldShow = ISingletonSetting<InScreenEditSetting>.Instance.ShowTimeIndicator.Value;
				indicatorActiveModifier.Register(isShow => shouldShow && isShow, 1);
			})
		};

		// Private
		private Modifier<bool> indicatorActiveModifier = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		private Camera levelCamera = default!;
		private NotifiableProperty<ISpeed> speed = default!;
		private IGameAudioPlayer music = default!;

		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);
		private string timeString = "0";
		private Modifier<Color>? colorModifier;
		private Modifier<string>? textModifier;

		// System Functions
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			[Key("stage")] Camera levelCamera,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			NotifiableProperty<ISpeed> speed,
			IGameAudioPlayer music)
		{
			this.levelInfo = levelInfo;
			this.levelCamera = levelCamera;
			this.timeRetriever = timeRetriever;
			this.speed = speed;
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		protected override void Awake()
		{
			base.Awake();
			indicatorActiveModifier = new(
				() => indicator.activeSelf,
				value => indicator.SetActive(value),
				_ => false);
			TextModifier.Register(_ => timeString, 0);
		}

		void Update()
		{
			if (!indicator.activeSelf) return;

			var mousePosition = Input.mousePosition;
			if (!levelCamera.ContainsScreenPoint(mousePosition))
			{
				indicator.transform.localPosition = new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold);
				return;
			}

			if (indicator.activeSelf && levelCamera.ScreenToWorldPoint(gamePlane, mousePosition, out var gamePoint))
			{
				var time = timeRetriever.Value.GetTimeStart(gamePoint);
				var y = (time - music.ChartTime) * speed.Value.SpeedRate;
				indicator.transform.localPosition = new(0, y);
				timeString = time.ToString();
				TextModifier.Update();
				timeText.transform.rotation = levelCamera.transform.rotation;
			}
		}
	}
}