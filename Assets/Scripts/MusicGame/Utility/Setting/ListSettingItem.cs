#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.ListRender;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public interface IListItemSettingItem<TData>
	{
		public TData Data { get; set; }

		public event Action<TData?, TData?>? OnListContentChanged;
	}

	public abstract class ListSettingItem<TData, TItem>
		: SingleValueSettingItem<List<TData>> where TItem : MonoBehaviour, IListItemSettingItem<TData>
	{
		// Serializable and Public
		[SerializeField] private TMP_Text descriptionText = default!;
		[SerializeField] private ListRendererInt listRenderer = default!;

		/// <summary> Will only be called once to initialize list renderer. </summary>
		protected abstract LazyPrefab ListItemPrefab { get; }

		protected override void InitializeSucceed()
		{
			var descriptionAttribute = TargetPropertyInfo!.GetCustomAttribute<DescriptionAttribute>();
			descriptionText.text = descriptionAttribute is null ? string.Empty : descriptionAttribute.Description;

			var maxLengthAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxLengthAttribute>();
			if (maxLengthAttribute is not null) maxLength = maxLengthAttribute.MaxLength;

			for (var i = 0; i < Math.Min(DisplayValue!.Count, maxLength); i++)
			{
				var data = DisplayValue![i];
				var item = listRenderer.Add<TItem>(i);
				item.Data = data;
				item.OnListContentChanged += ItemOnOnListContentChanged;
			}
		}

		protected override void InitializeFail()
		{
			descriptionText.text = $"Error fetching setting {FullClassName}.{PropertyName}";
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			foreach (var item in listRenderer.Values.Select(go => go.GetComponent<TItem>()))
			{
				item.OnListContentChanged -= ItemOnOnListContentChanged;
			}

			listRenderer.Clear();
			for (var i = 0; i < Math.Min(DisplayValue!.Count, maxLength); i++)
			{
				var data = DisplayValue![i];
				var item = listRenderer.Add<TItem>(i);
				item.Data = data;
				item.OnListContentChanged += ItemOnOnListContentChanged;
			}

			Save();
		}

		// Private
		private int maxLength = int.MaxValue;

		// Event Handlers
		private void ItemOnOnListContentChanged(TData? previous, TData? current)
		{
			bool shouldNotify = false;
			if (previous is not null)
			{
				if (current is null) shouldNotify = DisplayValue!.Remove(previous);
				else
				{
					var index = DisplayValue!.IndexOf(previous);
					if (index >= 0)
					{
						shouldNotify = true;
						DisplayValue[index] = current;
					}
				}
			}
			else if (current is not null && DisplayValue!.Count < maxLength)
			{
				shouldNotify = true;
				var index = DisplayValue!.IndexOf(current);
				if (index >= 0) DisplayValue.Insert(index + 1, current);
				else DisplayValue!.Add(current);
			}

			if (shouldNotify) ForceNotify();
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			listRenderer.Init(new() { [typeof(TItem)] = ListItemPrefab });
		}
	}
}