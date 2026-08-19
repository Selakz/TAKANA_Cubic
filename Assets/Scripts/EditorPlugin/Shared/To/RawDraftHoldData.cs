#nullable enable

using EditorPlugin.PluginSystem;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawDraftHoldData
	{
		public string type { get; }

		public IWrapper<int> id { get; }

		public IWrapper<string?> name { get; }

		public IWrapper<int> timeJudge { get; }

		public IWrapper<int> timeEnd { get; }

		public IWrapper<float> position { get; }

		public IWrapper<float> width { get; }

		public IWrapper<bool> isDummy { get; }

		public RawDraftHoldData(ChartComponent component, StagingRegistry? registry)
		{
			type = "DraftHold";
			id = new ValueWrapper<int>(() => component.Id, v => component.Id = v, registry);
			name = new ValueWrapper<string?>(() => component.Name, v => component.Name = v, registry);
			timeJudge = new PropertyWrapper<Hold, int>(component,
				h => h.TimeJudge.Milli,
				(h, v) => h.NudgeJudge(v - h.TimeJudge.Milli),
				registry);
			timeEnd = new PropertyWrapper<Hold, int>(component,
				h => h.TimeEnd.Milli,
				(h, v) => h.NudgeEnd(v - h.TimeEnd.Milli),
				registry);
			position = new PropertyWrapper<ISolitaryNote, float>(component,
				n => n.Position,
				(n, v) => n.Position = v,
				registry);
			width = new PropertyWrapper<ISolitaryNote, float>(component,
				n => n.Width,
				(n, v) => n.Width = v,
				registry);
			isDummy = new PropertyWrapper<Hold, bool>(component,
				h => h.IsDummy(),
				(h, v) => h.SetDummy(v),
				registry);
		}
	}
}
