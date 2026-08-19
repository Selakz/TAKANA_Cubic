#nullable enable

using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawHitData
	{
		public string type { get; }

		public IWrapper<int> id { get; }

		public IWrapper<string?> name { get; }

		public IWrapper<int> hitType { get; }

		public IWrapper<int> timeJudge { get; }

		public IWrapper<bool> isDummy { get; }

		public object track => api.GetComponentSnapshot(component.Parent!);

		private readonly ChartComponent component;
		private readonly ChartApi api;

		public RawHitData(ChartComponent component, StagingRegistry? registry, ChartApi api)
		{
			this.component = component;
			this.api = api;
			type = "Hit";
			id = new ValueWrapper<int>(() => component.Id, v => component.Id = v, registry);
			name = new ValueWrapper<string?>(() => component.Name, v => component.Name = v, registry);
			hitType = new HitTypeWrapper(component, registry);
			timeJudge = new PropertyWrapper<Hit, int>(component,
				h => h.TimeJudge.Milli,
				(h, v) =>
				{
					v = Mathf.Clamp(v, component.Parent!.Model.TimeMin, component.Parent.Model.TimeMax);
					h.Nudge(v - h.TimeJudge.Milli);
				},
				registry);
			isDummy = new PropertyWrapper<Hit, bool>(component,
				h => h.IsDummy(),
				(h, v) => h.SetDummy(v),
				registry);
		}

		private class HitTypeWrapper : PropertyWrapper<Hit, int>
		{
			public HitTypeWrapper(ChartComponent component, StagingRegistry? registry)
				: base(component, h => (int)h.Type, (h, v) => h.Type = (HitType)v, registry)
			{
			}

			protected override ICommand Factory()
			{
				var updateCommand = base.Factory();
				var chart = component.BelongingChart!;
				var parent = component.Parent;
				var hitTypeCommands = new[]
				{
					new DeleteComponentCommand(component),
					updateCommand,
					new AddComponentCommand(chart, component, parent)
				};
				return new BatchCommand(hitTypeCommands, "Update HitType");
			}
		}
	}
}