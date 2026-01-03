#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MusicGame.Models;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Gameplay.Chart
{
	// TODO: Make it IDataset or even more generic: ITreeDataset
	[ChartTypeMark("chart")]
	public class ChartInfo : IEnumerable<ChartComponent>, IChartSerializable, IDisposable
	{
		public ModelProperty Properties { get; set; } = new();

		public ModelProperty EditorConfig { get; set; } = new();

		public event Action<ChartComponent>? OnComponentAdded;
		public event Action<ChartComponent>? OnComponentRemoved;
		public event Action<ChartComponent>? OnComponentModelUpdated;

		/// <summary> The second parameter is its previous parent. </summary>
		public event Action<ChartComponent, ChartComponent?>? BeforeComponentParentChanged;

		private readonly HashSet<ChartComponent> components = new();
		private int generalId = 0;

		private const int VersionIdentifier = 2;

		public int NewId => generalId++;

		public ChartInfo()
		{
		}

		public ChartInfo(IEnumerable<IChartModel> models) : this()
		{
			foreach (var model in models) AddComponent(model);
		}

		public ChartComponent<TModel> AddComponentGeneric<TModel>(TModel model)
			where TModel : IChartModel
		{
			ChartComponent<TModel> component = new(model, this) { Id = generalId++ };
			return component;
		}

		public ChartComponent AddComponent(IChartModel model)
		{
			var modelType = model.GetType();
			var componentType = typeof(ChartComponent<>).MakeGenericType(modelType);
			var component = (ChartComponent)Activator.CreateInstance(
				type: componentType,
				bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
				binder: null,
				args: new object[] { model, this },
				culture: null
			);
			component.Id = generalId++;
			return component;
		}

		public void AddComponent(ChartComponent component)
		{
			component.BelongingChart = this;
		}

		internal void AddComponentInternal(ChartComponent component)
		{
			components.Add(component);
			OnComponentAdded?.Invoke(component);
			component.OnComponentUpdated += OnComponentUpdated;
			component.BeforeBelongingChartChanged += BeforeBelongingChartChanged;
			component.BeforeParentChanged += BeforeParentChanged;
		}

		/// <returns> components that are removed recursively in topological order. </returns>
		public List<ChartComponent> RemoveComponent(ChartComponent component)
		{
			List<ChartComponent> removed = new() { component };
			removed.AddRange(component.Descendants);
			component.BelongingChart = null;
			return removed;
		}

		public IEnumerator<ChartComponent> GetEnumerator() => components.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public JObject GetSerializationToken()
		{
			JObject dict = new();
			JArray componentArray = new();
			foreach (var component in components.Where(component => component.Parent is null))
			{
				componentArray.Add(component.GetSerializationToken());
			}

			dict.Add("version", VersionIdentifier);
			dict.AddIf("properties", Properties.GetSerializationToken(), Properties.Count > 0);
			dict.AddIf("editorconfig", EditorConfig.GetSerializationToken(), EditorConfig.Count > 0);
			dict.Add("components", componentArray);
			return dict;
		}

		public static ChartInfo Deserialize(JToken token)
		{
			ChartInfo chartInfo = new();
			if (token is not JObject dict || dict.Get("version", 0) != VersionIdentifier) return chartInfo;
			chartInfo.Properties = ModelProperty.Deserialize(dict["properties"] as JObject ?? new JObject());
			chartInfo.EditorConfig = ModelProperty.Deserialize(dict["editorconfig"] as JObject ?? new JObject());
			if (dict["components"] is JArray componentArray)
			{
				foreach (var componentToken in componentArray)
				{
					if (componentToken is not JObject componentObject) continue;
					// The components and its children will be automatically added to the chart.
					ChartComponent.Deserialize(componentObject, chartInfo);
				}
			}

			return chartInfo;
		}

		private void OnComponentUpdated(object sender, EventArgs e)
		{
			if (sender is not ChartComponent component) return;
			OnComponentModelUpdated?.Invoke(component);
		}

		private void BeforeBelongingChartChanged(object sender, ChartInfo? newChart)
		{
			if (sender is not ChartComponent component ||
			    component.BelongingChart != this ||
			    newChart == this) return;
			OnComponentRemoved?.Invoke(component);
			component.OnComponentUpdated -= OnComponentUpdated;
			component.BeforeBelongingChartChanged -= BeforeBelongingChartChanged;
			component.BeforeParentChanged -= BeforeParentChanged;
			components.Remove(component);
		}

		// ChartComponent assures that if new parent is in another chart, the BeforeBelongingChartChanged event is invoked first.
		private void BeforeParentChanged(object sender, ChartComponent? newParent)
		{
			if (sender is not ChartComponent component || component.BelongingChart != this) return;
			BeforeComponentParentChanged?.Invoke(component, newParent);
		}

		public void Dispose()
		{
			foreach (var component in components)
			{
				component.OnComponentUpdated -= OnComponentUpdated;
				component.BeforeBelongingChartChanged -= BeforeBelongingChartChanged;
				component.BeforeParentChanged -= BeforeParentChanged;
				component.BelongingChart = null;
			}

			components.Clear();
		}
	}
}