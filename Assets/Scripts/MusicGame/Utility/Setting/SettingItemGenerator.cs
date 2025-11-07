#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.ListRender;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class SettingItemGenerator : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private bool generateOnStart = true;
		[SerializeField] private ListRendererInt settingItemRenderer = default!;
		[SerializeField] private string settingClassName = string.Empty;

		public string SettingClassName
		{
			get => settingClassName;
			set => settingClassName = value;
		}

		// Private
		private Dictionary<Type, Type> settingTypeMap = default!;

		// Static

		// Defined Functions
		public bool Generate()
		{
			settingItemRenderer.Clear();
			var settingType = Type.GetType(SettingClassName);
			if (settingType is null)
			{
				Debug.LogWarning($"Could not find class of type {SettingClassName}");
				return false;
			}

			// 1. Check if the type is ISingletonSetting
			var singletonInterface = settingType.GetInterface("ISingletonSetting`1");
			if (singletonInterface is null)
			{
				Debug.LogWarning($"{SettingClassName} is not a singleton setting class");
				return false;
			}

			var genericArgs = singletonInterface.GetGenericArguments();
			if (genericArgs[0] != settingType)
			{
				Debug.LogWarning($"{SettingClassName} falsely implement ISingletonSetting<{genericArgs[0]}>");
				return false;
			}

			// 2. Get the type's all NotifiableProperty
			var propertyInfos = settingType.GetProperties();
			for (int i = 0; i < propertyInfos.Length; i++)
			{
				var propertyInfo = propertyInfos[i];

				// Skip properties with HideInGameAttribute.
				var hideInGame = propertyInfo.GetCustomAttribute<HideInGameAttribute>();
				if (hideInGame is not null) continue;

				var propertyType = propertyInfo.PropertyType;
				if (!propertyType.IsGenericType ||
				    propertyType.GetGenericTypeDefinition() != typeof(NotifiableProperty<>))
				{
					Debug.LogWarning($"{SettingClassName}.{propertyInfo.Name} is not a NotifiableProperty");
					continue;
				}

				// 3. Get the generic argument of the NotifiableProperty
				var args = propertyType.GetGenericArguments();
				if (args.Length != 1) continue;

				// 4. If having SettingValueConverterAttribute, get the converter
				var genericArg = propertyType.GetGenericArguments()[0];
				var converterAttribute = genericArg.GetCustomAttribute<SettingValueConverterAttribute>();
				if (converterAttribute is not null)
				{
					genericArg = converterAttribute.ToType;
				}

				// 4. If the type is in dictionary, generate it.
				if (settingTypeMap.TryGetValue(genericArg, out var settingItemType))
				{
					var item = settingItemRenderer.Add(settingItemType, i);
					if (item is ISettingItem settingItem)
					{
						settingItem.Initialize(SettingClassName, propertyInfo.Name);
					}
				}

				// 5. TODO: Refactor ListRenderer to be enable to add fallback items
			}

			return true;
		}

		public void Clear() => settingItemRenderer.Clear();

		// System Functions
		void Awake()
		{
			settingTypeMap = new()
			{
				[typeof(int)] = typeof(IntegerSettingItem),
				[typeof(bool)] = typeof(BoolSettingItem),
				[typeof(float)] = typeof(FloatSettingItem),
				[typeof(string)] = typeof(StringSettingItem),
				[typeof(T3Time)] = typeof(T3TimeSettingItem),
				[typeof(Color)] = typeof(ColorSettingItem),
				[typeof(List<Color?>)] = typeof(ColorListSettingItem)
			};
			Dictionary<Type, LazyPrefab> listPrefabs = new()
			{
				[typeof(IntegerSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/IntegerSettingItem", "IntegerSettingItemPrefab_OnLoad"),
				[typeof(BoolSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/BoolSettingItem", "BoolSettingItemPrefab_OnLoad"),
				[typeof(FloatSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/FloatSettingItem", "FloatSettingItemPrefab_OnLoad"),
				[typeof(StringSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/StringSettingItem", "StringSettingItemPrefab_OnLoad"),
				[typeof(T3TimeSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/T3TimeSettingItem", "T3TimeSettingItemPrefab_OnLoad"),
				[typeof(ColorSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/ColorSettingItem", "ColorSettingItemPrefab_OnLoad"),
				[typeof(ColorListSettingItem)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/ColorListSettingItem", "ColorListSettingItemPrefab_OnLoad")
			};
			settingItemRenderer.Init(listPrefabs);
		}

		void Start()
		{
			if (generateOnStart) Generate();
		}
	}
}