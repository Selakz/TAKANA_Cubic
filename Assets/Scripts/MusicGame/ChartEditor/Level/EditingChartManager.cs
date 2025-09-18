using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Components;
using MusicGame.Components.Chart;
using MusicGame.Components.JudgeLines;
using MusicGame.Components.Notes;
using MusicGame.Components.Tracks;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.Level
{
	public class EditingChartManager : MonoBehaviour, IEditingChartManager
	{
		// Serializable and Public
		public IJudgeLine DefaultJudgeLine { get; private set; }

		public ChartInfo Chart { get; private set; }

		// Private
		private readonly Dictionary<int, HashSet<EditingComponent>> subComponents = new();

		// Static

		// Defined Functions
		private static EditingComponent Encapsulate(IComponent component)
		{
			if (component is EditingComponent editingComponent) return editingComponent;
			return component switch
			{
				Track track => new EditingTrack(track),
				Tap tap => new EditingTap(tap),
				Slide slide => new EditingSlide(slide),
				Hold hold => new EditingHold(hold),
				_ => new EditingComponent(component),
			};
		}

		private bool CanAdd(EditingComponent component, IEnumerable<EditingComponent> supposedComponents = null)
		{
			if (Chart.Contains(component.Id) || component.Parent is null) return true;
			supposedComponents ??= Enumerable.Empty<EditingComponent>();
			return Chart.Contains(component.Parent.Id) ||
			       supposedComponents.Select(c => c.Id).Contains(component.Parent.Id);
		}

		public bool AddComponent(IComponent component)
		{
			var editingComponent = Encapsulate(component);
			if (!CanAdd(editingComponent, Enumerable.Empty<EditingComponent>())) return false;
			Chart.AddComponent(editingComponent);
			if (editingComponent.Parent is not null)
			{
				if (!subComponents.ContainsKey(editingComponent.Parent.Id))
					subComponents[editingComponent.Parent.Id] = new();
				subComponents[editingComponent.Parent.Id].Add(editingComponent);
			}

			InvokeUpdate();
			return true;
		}

		public bool AddComponents(IEnumerable<IComponent> components)
		{
			components = IComponent.TopologicalSort(components.ToList());
			List<EditingComponent> supposedComponents = new();
			var editingComponents = components.Select(Encapsulate);
			var enumerable = editingComponents as EditingComponent[] ?? editingComponents.ToArray();
			foreach (var component in enumerable)
			{
				if (!CanAdd(component, supposedComponents)) return false;
				supposedComponents.Add(component);
			}

			foreach (var component in enumerable)
			{
				Chart.AddComponent(component);
				if (component.Parent is not null)
				{
					if (!subComponents.ContainsKey(component.Parent.Id))
						subComponents[component.Parent.Id] = new();
					subComponents[component.Parent.Id].Add(component);
				}
			}

			InvokeUpdate();
			return true;
		}

		private IEnumerable<EditingComponent> RemoveInternal(int id)
		{
			var result = Enumerable.Empty<EditingComponent>();
			if (!Chart.Contains(id)) return result;
			result = result.Append(Chart[id] as EditingComponent);
			// 1. Get children components recursively (also removed its children)
			if (subComponents.TryGetValue(id, out var components))
			{
				result = components.Aggregate(result,
					(current, component) => current.Concat(RemoveInternal(component.Id)));
			}

			// 2. Remove it from its parent
			var parent = Chart[id]!.Parent;
			if (parent is not null && subComponents.ContainsKey(parent.Id))
			{
				subComponents[parent.Id] = subComponents[parent.Id].Where(component => component.Id != id).ToHashSet();
			}

			// 3. Remove it from subComponents
			subComponents.Remove(id);
			// 4. Remove it from chart
			Chart.RemoveComponent(id);
			return result;
		}

		public IEnumerable<EditingComponent> RemoveComponent(int id)
		{
			var result = RemoveInternal(id).ToList();
			InvokeUpdate();
			return result;
		}

		public IEnumerable<EditingComponent> RemoveComponents(IEnumerable<int> ids)
		{
			IEnumerable<EditingComponent> result = Enumerable.Empty<EditingComponent>();
			result = ids.Aggregate(result, (current, id) => current.Concat(RemoveInternal(id))).ToList();
			List<IComponent> editingComponents = result.Cast<IComponent>().ToList();
			editingComponents = IComponent.TopologicalSort(editingComponents).ToList();
			InvokeUpdate();
			return editingComponents.Cast<EditingComponent>();
		}

		// TODO: Change it with a string parameter indicating which property changed; invoke a new event
		public void UpdateProperties()
		{
			InvokeUpdate();
		}

		public void UpdateComponent(int id)
		{
			if (!Chart.Contains(id)) return;
			var component = Chart[id]!;
			Chart.RemoveComponent(id);
			Chart.AddComponent(component);
			InvokeUpdate();
		}

		public void UpdateComponents(IEnumerable<int> ids)
		{
			foreach (var id in ids)
			{
				if (!Chart.Contains(id)) continue;
				var component = Chart[id]!;
				Chart.RemoveComponent(id);
				Chart.AddComponent(component);
			}

			InvokeUpdate();
		}

		public IEnumerable<EditingComponent> GetChildrenComponents(int id)
		{
			return subComponents.TryGetValue(id, out var components)
				? components
				: Enumerable.Empty<EditingComponent>();
		}

		private void InvokeUpdate() => EventManager.Instance.Invoke("Chart_OnUpdate", Chart);

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			bool isDefaultJudgeLineSet = false;

			Chart = levelInfo.Chart;
			subComponents.Clear();
			var components = Chart.ToList();
			foreach (var editingComponent in components.Select(Encapsulate))
			{
				if (!isDefaultJudgeLineSet && editingComponent.Component is IJudgeLine judgeLine)
				{
					isDefaultJudgeLineSet = true;
					DefaultJudgeLine = judgeLine;
				}

				// Replace original component
				Chart.AddComponent(editingComponent);
				if (editingComponent.Parent is not null)
				{
					if (!subComponents.ContainsKey(editingComponent.Parent.Id))
						subComponents[editingComponent.Parent.Id] = new();
					subComponents[editingComponent.Parent.Id].Add(editingComponent);
				}
			}

			if (!isDefaultJudgeLineSet)
			{
				var judgeLine = new JudgeLine();
				AddComponent(judgeLine);
				DefaultJudgeLine = judgeLine;
			}
		}

		// System Functions
		void OnEnable()
		{
			IEditingChartManager.Instance = this;
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}