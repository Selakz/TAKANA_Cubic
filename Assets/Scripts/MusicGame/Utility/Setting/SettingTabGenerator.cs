#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;
using VContainer;
using IComponent = T3Framework.Runtime.ECS.IComponent;

namespace MusicGame.Utility.Setting
{
	public class SettingTabGenerator : HierarchySystem<SettingTabGenerator>
	{
		// Serializable and Public
		[SerializeField] private bool generateOnStart = true;
		[SerializeField] private ViewPoolInstaller tabInstaller = default!;
		[SerializeField] private ClassViewPoolInstaller<SettingItemClassType> itemInstaller = default!;
		[SerializeField] private List<string> settingClassNames = new();

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<SettingTabComponent>(tabViewPool, handler =>
				new SettingTabRegistrar(handler.Script<SettingTabToggle>(), tabViewPool[handler]!, itemViewPool), true)
		};

		// Private
		[Inject] private IViewPool<SettingTabComponent> tabViewPool = default!;
		[Inject] private IViewPool<SettingItemComponent> itemViewPool = default!;

		private readonly Dictionary<Type, SettingItemClassType> classTypeMap = new()
		{
			[typeof(int)] = SettingItemClassType.Integer,
			[typeof(bool)] = SettingItemClassType.Bool,
			[typeof(float)] = SettingItemClassType.Float,
			[typeof(string)] = SettingItemClassType.String,
			[typeof(T3Time)] = SettingItemClassType.T3Time,
			[typeof(Color)] = SettingItemClassType.Color,
			[typeof(List<Color?>)] = SettingItemClassType.ColorList
		};

		// Constructor
		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			tabInstaller.Register<ViewPool<SettingTabComponent>, SettingTabComponent>(builder, Lifetime.Singleton);
			itemInstaller.Register<ViewPool<SettingItemComponent, SettingItemClassType>, SettingItemComponent>(
				builder, Lifetime.Singleton, new SettingItemClassifier());
		}

		// Defined Functions
		public void Generate()
		{
			tabViewPool.Clear();
			itemViewPool.Clear();
			itemViewPool.IsGetActive = false;
			foreach (var settingClassName in settingClassNames)
			{
				var settingType = Type.GetType(settingClassName);
				if (settingType is null)
				{
					Debug.LogWarning($"Could not find class of type {settingClassName}");
					continue;
				}

				// 1. Check if the type is ISingletonSetting
				var singletonInterface = settingType.GetInterface("ISingletonSetting`1");
				if (singletonInterface is null)
				{
					Debug.LogWarning($"{settingClassName} is not a singleton setting class");
					continue;
				}

				var genericArgs = singletonInterface.GetGenericArguments();
				if (genericArgs[0] != settingType)
				{
					Debug.LogWarning($"{settingClassName} falsely implement ISingletonSetting<{genericArgs[0]}>");
					continue;
				}

				// 2. Generate it
				tabViewPool.Add(new SettingTabComponent(settingClassName));
				GenerateItems(settingClassName, settingType);
			}

			SyncActiveItems();
			return;

			void GenerateItems(string className, Type settingType)
			{
				var propertyInfos = settingType.GetProperties();
				foreach (var propertyInfo in propertyInfos)
				{
					// Skip properties with HideInGameAttribute.
					var hideInGame = propertyInfo.GetCustomAttribute<HideInGameAttribute>();
					if (hideInGame is not null) continue;

					var propertyType = propertyInfo.PropertyType;
					if (!propertyType.IsGenericType ||
					    propertyType.GetGenericTypeDefinition() != typeof(NotifiableProperty<>))
					{
						Debug.LogWarning($"{className}.{propertyInfo.Name} is not a NotifiableProperty");
						continue;
					}

					// 3. Get the generic argument of the NotifiableProperty
					var args = propertyType.GetGenericArguments();
					if (args.Length != 1) continue;

					// 4. If having SettingValueConverterAttribute, get the converter
					var genericArg = args[0];
					var converterAttribute = genericArg.GetCustomAttribute<SettingValueConverterAttribute>();
					if (converterAttribute is not null)
					{
						genericArg = converterAttribute.ToType;
					}

					// 5. If the type is in the map, generate it.
					if (!classTypeMap.TryGetValue(genericArg, out var classType)) continue;

					var component = new SettingItemComponent(className, propertyInfo.Name, classType);
					if (!itemViewPool.Add(component)) continue;
					itemViewPool[component]!.Script<ISettingItem>().Initialize(className, propertyInfo.Name);
				}
			}

			void SyncActiveItems()
			{
				Dictionary<string, bool> activeMap = new();
				foreach (var tabComponent in tabViewPool)
				{
					var toggle = tabViewPool[tabComponent]!.Script<SettingTabToggle>().Toggle;
					activeMap[tabComponent.Model] = toggle.isOn;
				}

				foreach (var itemComponent in itemViewPool)
				{
					itemViewPool[itemComponent]!.gameObject
						.SetActive(activeMap.GetValueOrDefault(itemComponent.SettingClassName));
				}
			}
		}

		// System Functions
		void Start()
		{
			if (generateOnStart) Generate();
		}
	}

	public class SettingTabComponent : BaseComponent<string>
	{
		public SettingTabComponent(string settingClassName) : base(settingClassName)
		{
		}
	}

	public class SettingItemComponent : IComponent
	{
		public string SettingClassName { get; }

		public string PropertyName { get; }

		public SettingItemClassType ClassType { get; }

		public event EventHandler? OnComponentUpdated;

		public SettingItemComponent(string settingClassName, string propertyName, SettingItemClassType classType)
		{
			SettingClassName = settingClassName;
			PropertyName = propertyName;
			ClassType = classType;
		}

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);
	}

	public enum SettingItemClassType
	{
		Integer,
		Bool,
		Float,
		String,
		T3Time,
		Color,
		ColorList
	}

	public class SettingItemClassifier : IClassifier<SettingItemClassType>
	{
		// Defined Functions
		public SettingItemClassType Classify(IComponent component) =>
			component is SettingItemComponent itemComponent ? itemComponent.ClassType : default;

		public bool IsOfType(IComponent component, SettingItemClassType type) => Classify(component) == type;

		public bool IsSubType(SettingItemClassType subType, SettingItemClassType type) => subType == type;
	}

	public class SettingTabRegistrar : CompositeRegistrar
	{
		// Private
		private readonly SettingTabToggle view;
		private readonly SettingTabComponent data;
		private readonly IViewPool<SettingItemComponent> itemViewPool;

		// Event Registrars
		protected override IEventRegistrar[] InnerRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(view.Toggle, OnToggleValueChanged)
		};

		// Constructor
		public SettingTabRegistrar(SettingTabToggle view, SettingTabComponent data,
			IViewPool<SettingItemComponent> itemViewPool)
		{
			this.view = view;
			this.data = data;
			this.itemViewPool = itemViewPool;
		}

		// Defined Functions
		protected override void Initialize()
		{
			var settingType = Type.GetType(data.Model);
			if (settingType is null) return;
			var description = settingType.GetCustomAttribute<DescriptionAttribute>();
			view.LabelTextBlock.SetText(description is null
				? data.Model
				: $"Setting_{settingType.Name}_{description.Description}");
		}

		protected override void Deinitialize()
		{
		}

		private void OnToggleValueChanged(bool value)
		{
			foreach (var component in itemViewPool)
			{
				if (component.SettingClassName != data.Model) continue;
				itemViewPool[component]!.gameObject.SetActive(value);
			}
		}
	}
}