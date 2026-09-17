#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Setting;
using UnityEngine;
using UnityEngine.UI;

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
		[SerializeField] private PrefabObject listItemPrefab = default!;
		[SerializeField] private RectTransform listContent = default!;

		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			var maxLengthAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxLengthAttribute>();
			if (maxLengthAttribute is not null) maxLength = maxLengthAttribute.MaxLength;

			RefreshItems();
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			RefreshItems();
			Save();
		}

		// Private
		private int maxLength = int.MaxValue;
		private ViewPool<BaseComponent<TData>>? listViewPool;
		private ViewPool<BaseComponent<TData>> ListViewPool => listViewPool ??= new(null, listItemPrefab, listContent);

		// Defined Functions
		private void RefreshItems()
		{
			foreach (var component in ListViewPool)
			{
				ListViewPool[component]!.Script<TItem>().OnListContentChanged -= ItemOnListContentChanged;
			}

			ListViewPool.Clear();
			for (var i = 0; i < Math.Min(DisplayValue!.Count, maxLength); i++)
			{
				var component = new BaseComponent<TData>(DisplayValue![i]);
				if (!ListViewPool.Add(component)) continue;
				var item = ListViewPool[component]!.Script<TItem>();
				item.transform.SetSiblingIndex(i);
				item.Data = component.Model;
				item.OnListContentChanged += ItemOnListContentChanged;
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(listContent);
		}

		// Event Handlers
		private void ItemOnListContentChanged(TData? previous, TData? current)
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
		protected override void OnDestroy()
		{
			base.OnDestroy();
			listViewPool?.Dispose();
		}
	}
}