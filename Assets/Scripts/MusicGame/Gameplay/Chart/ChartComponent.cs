#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicGame.Models;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Gameplay.Chart
{
	/// <summary> Generally use <see cref="ChartComponent{T}"/> instead of this to form a component. </summary>
	[ChartTypeMark("component")]
	public class ChartComponent : IChartSerializable, IComponent<IChartModel>
	{
		public ChartInfo? BelongingChart
		{
			get => belongingChart;
			set
			{
				if (belongingChart == value) return;
				SetParent(null);
				SetNewChartRecursively(this, value);
				return;

				static void SetNewChartRecursively(ChartComponent component, ChartInfo? newChart)
				{
					foreach (var child in component.Children) SetNewChartRecursively(child, newChart);
					component.BeforeBelongingChartChanged?.Invoke(component, newChart);
					newChart?.AddComponentInternal(component);
					component.belongingChart = newChart;
				}
			}
		}

		public IChartModel Model { get; }

		public int Id
		{
			get => id;
			set
			{
				id = value;
				TriggerUpdate();
			}
		}

		public string? Name
		{
			get => name;
			set
			{
				name = value;
				TriggerUpdate();
			}
		}

		public event EventHandler? OnComponentUpdated;
		public event EventHandler<ChartInfo?>? BeforeBelongingChartChanged;
		public event EventHandler<ChartComponent?>? BeforeParentChanged;

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		/// <summary> Use <see cref="SetParent"/> to avoid throwing exception </summary>
		public ChartComponent? Parent
		{
			get => parent;
			set
			{
				if (!SetParent(value)) throw new InvalidOperationException("Set parent failed.");
			}
		}

		public IReadOnlyCollection<ChartComponent> Children => children;

		public IEnumerable<ChartComponent> Descendants => Children.Concat(Children.SelectMany(c => c.Descendants));

		private ChartInfo? belongingChart;
		private ChartComponent? parent;
		private readonly HashSet<ChartComponent> children = new();
		private int id;
		private string? name;

		public ChartComponent(IChartModel model, ChartInfo? belongingChart = null)
		{
			Model = model;
			BelongingChart = belongingChart;
		}

		public void UpdateModel(Action<IChartModel> action)
		{
			action.Invoke(Model);
			TriggerUpdate();
		}

		public bool Nudge(T3Time distance)
		{
			if (!IsWithinParentRange(distance)) return false;
			foreach (var child in Children) child.Nudge(distance);
			UpdateModel(model => model.Nudge(distance));
			return true;
		}

		public bool SetParent(ChartComponent? newParent)
		{
			if (parent == newParent) return true;
			var previousParent = parent;
			if (newParent is null)
			{
				BeforeParentChanged?.Invoke(this, newParent);
				previousParent?.children.Remove(this);
				parent = null;
				return true;
			}

			BelongingChart = newParent.BelongingChart;
			BeforeParentChanged?.Invoke(this, newParent);
			ChartComponent? ancestor = newParent;
			while (ancestor is not null)
			{
				if (ancestor == this) return false;
				ancestor = ancestor.Parent;
			}

			parent?.children.Remove(this);
			parent = newParent;
			newParent.children.Add(this);
			return true;
		}

		public bool HasChild(ChartComponent component) => children.Contains(component);

		public bool AddChild(ChartComponent component) => component.SetParent(this);

		public bool IsNewTimeMinValid(T3Time newTimeMin)
		{
			if (newTimeMin > Model.TimeMax) return false; // TODO: It;s not quite right for notes
			if (Children.Count > 0 && newTimeMin > Children.Min(c => c.Model.TimeMin)) return false;
			if (Parent is not null && newTimeMin < Parent.Model.TimeMin) return false;
			return true;
		}

		public bool IsNewTimeMaxValid(T3Time newTimeMax)
		{
			if (newTimeMax < Model.TimeMin) return false;
			if (Children.Count > 0 && newTimeMax < Children.Max(c => c.Model.TimeMax)) return false;
			if (Parent is not null && newTimeMax > Parent.Model.TimeMax) return false;
			return true;
		}

		public bool IsWithinRange(ChartComponent? parent, T3Time distance)
		{
			if (parent is null) return true;
			if (Model.TimeMin + distance < parent.Model.TimeMin) return false;
			if (Model.TimeMax + distance > parent.Model.TimeMax) return false;
			return true;
		}

		public bool IsWithinParentRange(T3Time distance) => IsWithinRange(Parent, distance);

		public JObject GetSerializationToken()
		{
			JObject dict = new();
			dict.Add("id", Id);
			dict.AddIf("name", Name, !string.IsNullOrEmpty(Name));
			dict.Add("model", Model.Serialize(true));
			JArray childList = new();
			foreach (var child in children) childList.Add(child.GetSerializationToken());
			dict.AddIf("children", childList, children.Count > 0);
			return dict;
		}

		internal static ChartComponent Deserialize(JObject dict, object chartObject)
		{
			ChartInfo? chart = null;
			if (chartObject is ChartInfo chartInfo) chart = chartInfo;
			var id = dict.Get("id", -1);
			string? name = dict.Get<string>("name");
			var model = IChartSerializable.Deserialize((dict["model"] as JObject)!);
			if (model is not IChartModel chartModel) throw new InvalidDataException();

			ChartComponent self = new(chartModel, chart)
			{
				Id = id,
				Name = name
			};

			if (dict["children"] is JArray childList)
			{
				foreach (var childToken in childList)
				{
					if (childToken is not JObject childObject) continue;
					var child = Deserialize(childObject, chartObject);
					child.Parent = self;
				}
			}

			return self;
		}

		protected void TriggerUpdate() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);
	}

	public sealed class ChartComponent<TModel> : ChartComponent where TModel : IChartModel
	{
		public new TModel Model { get; }

		internal ChartComponent(TModel model, ChartInfo belongingChart) :
			base(model, belongingChart)
		{
			Model = model;
		}

		public void UpdateModel(Action<TModel> action)
		{
			action.Invoke(Model);
			TriggerUpdate();
		}
	}
}