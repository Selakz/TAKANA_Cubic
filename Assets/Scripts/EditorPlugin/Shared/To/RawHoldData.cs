#nullable enable

using EditorPlugin.PluginSystem;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawHoldData
	{
		public string type { get; }

		public IWrapper<int> id { get; }

		public IWrapper<string?> name { get; }

		public IWrapper<int> timeJudge { get; }

		public IWrapper<int> timeEnd { get; }

		public IWrapper<bool> isDummy { get; }

		public object track => api.GetComponentSnapshot(component.Parent!);

		private readonly ChartComponent component;
		private readonly ChartApi api;

		public RawHoldData(ChartComponent component, StagingRegistry? registry, ChartApi api)
		{
			this.component = component;
			this.api = api;
			type = "Hold";
			id = new ValueWrapper<int>(() => component.Id, v => component.Id = v, registry);
			name = new ValueWrapper<string?>(() => component.Name, v => component.Name = v, registry);
			timeJudge = new PropertyWrapper<Hold, int>(component,
				h => h.TimeJudge.Milli,
				(h, v) =>
				{
					v = Mathf.Clamp(v, component.Parent!.Model.TimeMin, h.TimeEnd - 1);
					h.NudgeJudge(v - h.TimeJudge.Milli);
				},
				registry);
			timeEnd = new PropertyWrapper<Hold, int>(component,
				h => h.TimeEnd.Milli,
				(h, v) =>
				{
					v = Mathf.Clamp(v, h.TimeJudge + 1, component.Parent!.Model.TimeMax);
					h.NudgeEnd(v - h.TimeEnd.Milli);
				},
				registry);
			isDummy = new PropertyWrapper<Hold, bool>(component,
				h => h.IsDummy(),
				(h, v) => h.SetDummy(v),
				registry);
		}
	}
}