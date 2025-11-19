#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class PositionIndicator : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<LevelInfo?> levelInfoContainer = default!;
		[SerializeField] private NotifiableDataContainer<float> cameraRotationContainer = default!;
		[SerializeField] private TMP_Text positionText = default!;
		[SerializeField] private GameObject indicator = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<LevelInfo?>(levelInfoContainer, (_, _) =>
			{
				var levelInfo = levelInfoContainer.Property.Value;
				indicatorActiveModifier.Register(_ => levelInfo is not null, 0);
			}),
			new PropertyRegistrar<bool>(ISingletonSetting<InScreenEditSetting>.Instance.ShowPositionIndicator, (_, _) =>
			{
				var shouldShow = ISingletonSetting<InScreenEditSetting>.Instance.ShowPositionIndicator.Value;
				indicatorActiveModifier.Register(isShow => shouldShow && isShow, 1);
			})
		};

		// Private
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		private Modifier<bool> indicatorActiveModifier = default!;

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			indicatorActiveModifier = new(
				() => indicator.activeSelf,
				value => indicator.SetActive(value),
				_ => false);
		}

		void Update()
		{
			if (!indicator.activeSelf) return;

			var levelCamera = LevelManager.Instance.LevelCamera;
			var mousePosition = Input.mousePosition;
			if (!levelCamera.ContainsScreenPoint(mousePosition))
			{
				indicator.transform.localPosition = new(10000, 0);
				return;
			}

			if (indicator.activeSelf && levelCamera.ScreenToWorldPoint(gamePlane, mousePosition, out var gamePoint))
			{
				var position = InScreenEditManager.Instance.WidthRetriever.GetAttachedPosition(gamePoint);
				indicator.transform.localPosition = new(position, 0);
				positionText.text = position.ToString("0.00");
				positionText.transform.rotation = new Quaternion(cameraRotationContainer.Property.Value, 0, 0, 1f);
			}
		}
	}
}