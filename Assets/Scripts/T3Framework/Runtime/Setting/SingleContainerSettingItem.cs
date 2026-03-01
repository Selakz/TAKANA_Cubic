#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace T3Framework.Runtime.Setting
{
	// The worst news: you need to create a solitary file for every inheritor of a generic MonoBehaviour
	public class SingleContainerSettingItem<T> : SingleValueSettingItem<T>
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<T> valueDataContainer = default!;
		[SerializeField] private Transform inputRoot = default!;
		[SerializeField] private InspectorDictionary<InspectorType, PrefabObject> inputValuePrefabs = new();
		[SerializeField] private PrefabObject fallbackPrefab = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<T>(valueDataContainer.Property, value =>
			{
				if (Equals(value, DisplayValue)) return;
				DisplayValue = valueDataContainer.Property.Value;
				Save();
			})
		};

		// Private
		private Dictionary<Type, PrefabObject>? prefabs;

		private Dictionary<Type, PrefabObject> Prefabs => prefabs ??=
			new(inputValuePrefabs.Value.Select(pair =>
				new KeyValuePair<Type, PrefabObject>(pair.Key.Type, pair.Value)));

		// Defined Functions
		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			bool fallback = true;
			foreach (var attribute in TargetPropertyInfo!.GetCustomAttributes(true))
			{
				if (Prefabs.TryGetValue(attribute.GetType(), out var prefab))
				{
					fallback = false;
					prefab.SimpleInstantiate(inputRoot);
				}
			}

			if (fallback) fallbackPrefab.SimpleInstantiate(inputRoot);
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (DisplayValue is { } value) valueDataContainer.Property.Value = value;
		}
	}
}