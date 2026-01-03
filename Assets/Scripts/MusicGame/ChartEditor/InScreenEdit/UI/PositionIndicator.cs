#nullable enable

using MusicGame.Gameplay.Level;
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
	public class PositionIndicator : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_Text positionText = default!;
		[SerializeField] private GameObject indicator = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				IndicatorActiveModifier.Register(_ => info is not null, 0);
			}),
			new PropertyRegistrar<bool>(ISingletonSetting<InScreenEditSetting>.Instance.ShowPositionIndicator, (_, _) =>
			{
				var shouldShow = ISingletonSetting<InScreenEditSetting>.Instance.ShowPositionIndicator.Value;
				IndicatorActiveModifier.Register(isShow => shouldShow && isShow, 1);
			})
		};

		// Private
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		private Modifier<bool> IndicatorActiveModifier => indicatorActiveModifier ??= new(
			() => indicator.activeSelf,
			value => indicator.SetActive(value),
			_ => false);

		private Modifier<bool>? indicatorActiveModifier;
		private Camera levelCamera = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<IWidthRetriever> widthRetriever = default!;

		// Defined Functions
		[Inject]
		public void Construct(
			[Key("stage")] Camera levelCamera,
			NotifiableProperty<LevelInfo?> levelInfo,
			NotifiableProperty<IWidthRetriever> widthRetriever)
		{
			this.levelCamera = levelCamera;
			this.levelInfo = levelInfo;
			this.widthRetriever = widthRetriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// System Functions
		void Update()
		{
			if (!indicator.activeSelf) return;

			var mousePosition = Input.mousePosition;
			if (!levelCamera.ContainsScreenPoint(mousePosition))
			{
				indicator.transform.localPosition = new(10000, 0);
				return;
			}

			if (indicator.activeSelf && levelCamera.ScreenToWorldPoint(gamePlane, mousePosition, out var gamePoint))
			{
				var position = widthRetriever.Value.GetAttachedPosition(gamePoint);
				indicator.transform.localPosition = new(position, 0);
				positionText.text = position.ToString("0.00");
				positionText.transform.rotation = levelCamera.transform.rotation;
			}
		}
	}
}