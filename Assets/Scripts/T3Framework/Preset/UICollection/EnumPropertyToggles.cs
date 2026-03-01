#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	public abstract class EnumPropertyToggles<T> : T3MonoBehaviour where T : Enum
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<T, Toggle> toggles = default!;

		protected abstract NotifiableProperty<T> EnumProperty { get; }

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new UnionRegistrar(ToggleRegistrars),
			new PropertyRegistrar<T>(EnumProperty, () =>
			{
				foreach (var pair in toggles.Value)
				{
					pair.Value.SetIsOnWithoutNotify(EnumProperty.Value.Equals(pair.Key));
				}
			})
		};

		// Defined Functions
		private IEnumerable<IEventRegistrar> ToggleRegistrars()
		{
			foreach (var pair in toggles.Value)
			{
				yield return new ToggleRegistrar(pair.Value, isOn =>
				{
					if (!isOn) return;
					EnumProperty.Value = pair.Key;
				});
			}
		}
	}
}