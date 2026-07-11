#nullable enable

using System.Collections.Generic;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.Gameplay.Stage.UI
{
	public class StageSkinConfigToggles : HierarchySystem<StageSkinConfigToggles>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<Toggle, GameplayStageSkinConfig> toggles = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars
		{
			get
			{
				List<IEventRegistrar> registrars = new(toggles.Value.Count);
				foreach (var (toggle, skinConfig) in toggles.Value)
				{
					registrars.Add(new ToggleRegistrar(toggle, isOn =>
					{
						if (isOn) config.Value = skinConfig;
					}));
				}

				return registrars.ToArray();
			}
		}

		// Private
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> config = default!;
	}
}