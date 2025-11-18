#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TimeIndicator : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<LevelInfo?> levelInfoContainer = default!;
		[SerializeField] private NotifiableDataContainer<ISpeed> speedContainer = default!;
		[SerializeField] private NotifiableDataContainer<float> cameraRotationContainer = default!;
		[SerializeField] private TMP_Text timeText = default!;
		[SerializeField] private GameObject indicator = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<LevelInfo?>(levelInfoContainer, (_, _) =>
			{
				var levelInfo = levelInfoContainer.Property.Value;
				indicator.SetActive(levelInfo is not null);
			})
		};

		// Private
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		// System Functions
		void Update()
		{
			if (!indicator.activeSelf) return;

			var levelCamera = LevelManager.Instance.LevelCamera;
			var mousePosition = Input.mousePosition;
			if (!levelCamera.ContainsScreenPoint(mousePosition))
			{
				indicator.transform.localPosition = new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold);
				return;
			}

			if (indicator.activeSelf && levelCamera.ScreenToWorldPoint(gamePlane, mousePosition, out var gamePoint))
			{
				var time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
				var y = (time - LevelManager.Instance.Music.ChartTime) * speedContainer.Property.Value.SpeedRate;
				indicator.transform.localPosition = new(0, y);
				timeText.text = time.ToString();
				timeText.transform.rotation = new Quaternion(cameraRotationContainer.Property.Value, 0, 0, 1f);
			}
		}
	}
}