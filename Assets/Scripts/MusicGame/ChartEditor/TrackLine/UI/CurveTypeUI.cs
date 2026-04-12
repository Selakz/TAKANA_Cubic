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

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class CurveTypeUI : HierarchySystem<CurveTypeUI>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<NodeCurveType, Toggle> toggles = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<NodeCurveType>(curveType,
				type => toggles.Value[type].SetIsOnWithoutNotify(true)),
			new UnionRegistrar(() =>
			{
				List<IEventRegistrar> registrars = new(toggles.Value.Count);
				foreach (var (type, toggle) in toggles.Value)
				{
					registrars.Add(new ToggleRegistrar(toggle, isOn =>
					{
						if (isOn) curveType.Value = type;
					}));
				}

				return registrars;
			})
		};

		// Private
		[Inject] private readonly NotifiableProperty<NodeCurveType> curveType = default!;
	}
}