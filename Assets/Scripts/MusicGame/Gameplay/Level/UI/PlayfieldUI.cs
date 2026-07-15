#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace MusicGame.Gameplay.Level.UI
{
	public class PlayfieldUI : HierarchySystem<PlayfieldUI>
	{
		// Serializable and Public
		[SerializeField] private DoubleClickButton restartButton = default!;
		[SerializeField] private GameObject restartFirstClickIndicator = default!;
		[SerializeField] private DoubleClickButton homeButton = default!;
		[SerializeField] private GameObject homeFirstClickIndicator = default!;
		[SerializeField] private int sceneIndex = 0;
		[SerializeField] private GameObject autoPlayIndicator = default!;
		[SerializeField] private TMP_Text pitchText = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DoubleClickButtonRegistrar(restartButton, DoubleClickButtonRegistrar.RegisterTarget.First,
				() => restartFirstClickIndicator.SetActive(true)),
			new DoubleClickButtonRegistrar(restartButton, DoubleClickButtonRegistrar.RegisterTarget.FirstCancelled,
				() => restartFirstClickIndicator.SetActive(false)),
			new DoubleClickButtonRegistrar(restartButton, DoubleClickButtonRegistrar.RegisterTarget.Second, () =>
			{
				if (levelInfo.Value is not null) levelInfo.ForceNotify();
				restartFirstClickIndicator.SetActive(false);
			}),
			new DoubleClickButtonRegistrar(homeButton, DoubleClickButtonRegistrar.RegisterTarget.First,
				() => homeFirstClickIndicator.SetActive(true)),
			new DoubleClickButtonRegistrar(homeButton, DoubleClickButtonRegistrar.RegisterTarget.FirstCancelled,
				() => homeFirstClickIndicator.SetActive(false)),
			new DoubleClickButtonRegistrar(homeButton, DoubleClickButtonRegistrar.RegisterTarget.Second, () =>
			{
				homeFirstClickIndicator.SetActive(false);
				SceneManager.LoadScene(sceneIndex);
			}),
			new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
			{
				if (info?.Preference is GameplayPreference preference)
				{
					autoPlayIndicator.SetActive(preference.IsAuto);
					pitchText.gameObject.SetActive(!Mathf.Approximately(preference.Pitch, 1));
					pitchText.text = $"x{preference.Pitch:0.00}";
				}
			})
		};

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
	}
}