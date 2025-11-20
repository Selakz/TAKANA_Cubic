#nullable enable

using System;
using System.ComponentModel;
using System.Reflection;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace T3Framework.Runtime.Setting
{
	public abstract class SingleValueSettingItem<T> : T3MonoBehaviour, ISettingItem
	{
		// Serializable and Public
		public T? DisplayValue
		{
			get
			{
				if (targetProperty is null) return default;
				object result = targetProperty.GenericValue;
				if (settingValueConverter is not null)
				{
					result = settingValueConverter.Value.FromActualToDisplay(result);
					if (result.GetType() != typeof(T))
					{
						Debug.LogError($"Failed to convert {actualSettingType} to {typeof(T)}");
						return default;
					}
				}

				return (T?)result;
			}
			set
			{
				if (targetProperty is null || value is null) return;
				object result = value;
				if (settingValueConverter is not null)
				{
					result = settingValueConverter.Value.FromDisplayToActual.Invoke(result);
					if (result.GetType() != actualSettingType)
					{
						Debug.LogError($"Failed to convert {typeof(T)} to {actualSettingType}");
						return;
					}
				}

				targetProperty.GenericValue = result;
			}
		}

		/// <summary> If not null, it's guaranteed to be <see cref="ISingletonSetting{T}"/>. </summary>
		public Type? SettingType { get; private set; }

		public PropertyInfo? TargetPropertyInfo { get; private set; }

		public string FullClassName { get; private set; } = string.Empty;

		public string PropertyName { get; private set; } = string.Empty;

		// Private
		private Type? actualSettingType;
		private SettingValueConverter? settingValueConverter;
		private INotifiableProperty? targetProperty;

		// Defined Functions
		public void Initialize(string fullClassName, string propertyName)
		{
			if (targetProperty is not null)
			{
				targetProperty.PropertyChanged -= OnPropertyValueChanged;
			}

			if (Register(fullClassName, propertyName))
			{
				if (isActiveAndEnabled) targetProperty!.PropertyChanged += OnPropertyValueChanged;
				InitializeSucceed();
			}
			else
			{
				Debug.LogError($"Failed to register {fullClassName}.{propertyName}.");
				InitializeFail();
			}
		}

		public void Save()
		{
			var saveMethod = SettingType?.GetMethod("SaveInstance", BindingFlags.Static | BindingFlags.Public);
			saveMethod?.Invoke(null, Array.Empty<object>());
		}

		public void ForceNotify()
		{
			targetProperty?.ForceNotify();
		}

		private bool Register(string fullClassName, string propertyName)
		{
			FullClassName = fullClassName;
			PropertyName = propertyName;

			// 1. Get the type
			Type? type = Type.GetType(FullClassName);
			if (type is null)
			{
				Debug.LogWarning($"Could not find class of type {fullClassName}");
				return false;
			}

			// 2. See if the type implements ISingletonSetting
			var singletonInterface = type.GetInterface("ISingleton`1");
			if (singletonInterface is null)
			{
				Debug.LogWarning($"{fullClassName} is not a singleton setting class");
				return false;
			}

			var genericArgs = singletonInterface.GetGenericArguments();
			if (genericArgs[0] != type)
			{
				Debug.LogWarning($"{fullClassName} falsely implement ISingleton<{genericArgs[0]}>");
				return false;
			}

			var singletonSettingInterface = type.GetInterface("ISingletonSetting`1");
			if (singletonSettingInterface is null)
			{
				Debug.LogWarning($"{fullClassName} is not a singleton setting class");
				return false;
			}

			SettingType = singletonSettingInterface;

			// 3. Get the type's instance
			var instanceProperty = singletonInterface.GetProperty(
				"Instance",
				BindingFlags.Public | BindingFlags.Static,
				null,
				type,
				Type.EmptyTypes,
				null);
			var instance = instanceProperty?.GetValue(null);
			if (instance is null)
			{
				Debug.LogWarning($"Failed to get {fullClassName}'s instance");
				return false;
			}

			// 4. Get the target NotifiableProperty
			var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if (propertyInfo is null)
			{
				Debug.LogWarning($"Failed to get {fullClassName}'s property {propertyName}");
				return false;
			}

			TargetPropertyInfo = propertyInfo;
			var propertyType = propertyInfo.PropertyType;
			if (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(NotifiableProperty<>))
			{
				Debug.LogWarning($"{fullClassName}.{propertyName} is not a {typeof(NotifiableProperty<>)}");
				return false;
			}

			// 5. If having SettingValueConverterAttribute, get the converter
			var genericArg = propertyType.GetGenericArguments()[0];
			var converterAttribute = genericArg.GetCustomAttribute<SettingValueConverterAttribute>();
			if (converterAttribute is not null)
			{
				actualSettingType = genericArg;
				settingValueConverter = ISettingItem.GetValueConverter(converterAttribute.ConverterName);
				if (settingValueConverter is null)
				{
					Debug.LogWarning($"Cannot find SettingValueConverter of name {converterAttribute.ConverterName}");
					return false;
				}
			}
			else if (genericArg != typeof(T))
			{
				Debug.LogWarning($"{fullClassName}.{propertyName} is not a {typeof(NotifiableProperty<T>)})");
				return false;
			}

			// 6. Get the property
			var property = propertyInfo.GetValue(instance);
			if (property is not INotifiableProperty notifiableProperty)
			{
				Debug.LogWarning($"{fullClassName}.{propertyName} is not a INotifiableProperty");
				return false;
			}

			targetProperty = notifiableProperty;
			return true;
		}

		/// <summary>
		/// Initialization is successful and all properties are finely initialized.
		/// </summary>
		protected abstract void InitializeSucceed();

		/// <summary>
		/// Initialization failed, the target property may be null.
		/// </summary>
		protected abstract void InitializeFail();

		/// <summary>
		/// The action when the target property's value changed.
		/// </summary>
		protected abstract void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e);

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			if (targetProperty is not null)
			{
				targetProperty.PropertyChanged -= OnPropertyValueChanged;
				targetProperty.PropertyChanged += OnPropertyValueChanged;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (targetProperty is not null)
			{
				targetProperty.PropertyChanged -= OnPropertyValueChanged;
			}
		}
	}
}