#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.LevelSelect.UI
{
	[Serializable]
	public struct DifficultyToggleData
	{
		public Toggle toggle;
		public Image indicatorImage;
		public TextMeshProUGUI labelText;
	}

	public class DifficultyToggleGroupUI : HierarchySystem<DifficultyToggleGroupUI>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<int, DifficultyToggleData> toggles = default!;
		[SerializeField] private DifficultyConfig difficultyConfig = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars
		{
			get
			{
				var registrars = new List<IEventRegistrar>
				{
					new PropertyRegistrar<int>(difficulty, diff =>
					{
						foreach (var (key, data) in toggles.Value)
						{
							data.toggle.SetIsOnWithoutNotify(key == diff);
						}
					}),
					new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(rawLevelInfo, UpdateLabels)
				};

				foreach (var (diff, data) in toggles.Value)
				{
					registrars.Add(new ToggleRegistrar(data.toggle, isOn =>
					{
						if (isOn) difficulty.Value = diff;
					}));
				}

				return registrars.ToArray();
			}
		}

		// Private
		[Inject] private NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo = default!;
		[Inject] private NotifiableProperty<int> difficulty = default!;

		// Event Handlers
		private void UpdateLabels(RawLevelInfo<GameplayPreference>? info)
		{
			var difficulties = info?.SongInfo.Value?.Difficulties;

			foreach (var (diff, data) in toggles.Value)
			{
				data.labelText.text = difficulties is not null && difficulties.TryGetValue(diff, out var diffInfo)
					? diffInfo.LevelDisplay
					: "-";

				if (difficultyConfig.Value.TryGetValue(diff, out var configData))
				{
					data.indicatorImage.color = configData.color;
				}
			}
		}
	}
}