#nullable enable

using System.Collections.Generic;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class TrackTypeToggles : HierarchySystem<TrackTypeToggles>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<TrackType, Toggle> toggles = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<TrackType>(service.SelectedTrackType,
				type => toggles.Value[type].SetIsOnWithoutNotify(true)),
			new UnionRegistrar(() =>
			{
				List<IEventRegistrar> registrars = new(toggles.Value.Count);
				foreach (var (trackType, toggle) in toggles.Value)
				{
					registrars.Add(new ToggleRegistrar(toggle, isOn =>
					{
						if (isOn) service.SelectedTrackType.Value = trackType;
					}));
				}

				return registrars;
			})
		};

		// Private
		[Inject] private readonly ChartEditSystem service = default!;
	}
}