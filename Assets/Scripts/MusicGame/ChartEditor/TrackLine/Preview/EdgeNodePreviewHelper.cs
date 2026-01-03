#nullable enable

using System.Collections.Generic;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Preview
{
	public class EdgeNodePreviewData
	{
		public T3Time Time { get; set; }

		public float Position { get; set; }

		public Eases Ease { get; set; } = Eases.Unmove;

		public PositionNodeView View { get; }

		public EdgeNodePreviewData(PositionNodeView view)
		{
			View = view;
		}
	}

	public class EdgeNodePreviewHelper
	{
		private readonly SortedList<T3Time, float> times = new();
		private bool isShow;
		private Transform? rootTransform;

		public IReadOnlyCollection<EdgeNodePreviewData> Data { get; }
		public EdgePMLComponent? MoveList { get; set; }

		public bool IsShow
		{
			get => isShow;
			set
			{
				isShow = value;
				if (!isShow)
				{
					foreach (var data in Data) data.View.gameObject.SetActive(false);
					HeadNode?.gameObject.SetActive(false);
				}
				else UpdateView();
			}
		}

		public Transform? RootTransform
		{
			get => rootTransform;
			set
			{
				if (value == rootTransform) return;
				rootTransform = value;
				foreach (var data in Data) data.View.transform.SetParent(rootTransform, false);
				HeadNode?.transform.SetParent(rootTransform, false);
			}
		}

		public int AlphaPriority { get; set; } = int.MaxValue - 1;
		public float Alpha { get; set; } = 1f;

		public int IllegalColorPriority { get; set; } = int.MaxValue;
		public Color IllegalColor { get; set; } = Color.red;

		public float SpeedRate { get; set; }

		public PositionNodeView? HeadNode { get; set; }

		/// <summary> It's practical for caller to externally update the given collection. </summary>
		public EdgeNodePreviewHelper(IReadOnlyList<EdgeNodePreviewData> data)
		{
			Data = data;
			if (data.Count > 0)
			{
				isShow = data[0].View.gameObject.activeSelf;
				rootTransform = data[0].View.transform;
			}
		}

		public void UpdateView()
		{
			if (!isShow || MoveList is null) return;

			times.Clear();
			foreach (var pair in MoveList.Model) times[pair.Key] = pair.Value.Position;
			T3Time dataMinTime = T3Time.MaxValue;
			foreach (var data in Data)
			{
				dataMinTime = Mathf.Min(dataMinTime, data.Time);

				data.View.gameObject.SetActive(true);
				data.View.ColorModifier.Register(color => color with { a = Alpha }, AlphaPriority);
				if (times.ContainsKey(data.Time) ||
				    times.ContainsKey(data.Time - 1) ||
				    times.ContainsKey(data.Time + 1))
				{
					data.View.ColorModifier.Register(color => IllegalColor with { a = color.a }, IllegalColorPriority);
				}
				else data.View.ColorModifier.Unregister(IllegalColorPriority);

				times[data.Time] = data.Position;
			}

			var track = MoveList.Locator.Track.Model;
			foreach (var data in Data)
			{
				data.View.transform.SetParent(RootTransform, false);

				var index = times.BinarySearch(data.Time);
				if (index < 0) continue; // it should be impossible
				var x = data.Position;
				var y = (data.Time - track.TimeMin).Second * SpeedRate;
				if (index == times.Count - 1) data.View.Init(data.Ease, new(x, y), new(x, y));
				else
				{
					var nextTime = times.Keys[index + 1];
					var nextX = times.Values[index + 1];
					var nextY = (nextTime - track.TimeMin).Second * SpeedRate;
					data.View.Init(data.Ease, new(x, y), new(nextX, nextY));
				}
			}

			if (HeadNode is not null && dataMinTime != T3Time.MaxValue)
			{
				HeadNode.transform.SetParent(RootTransform, false);
				HeadNode.ColorModifier.Register(color => color with { a = Alpha }, AlphaPriority);
				bool active = true;
				var headIndex = MoveList.Model.BinarySearch(dataMinTime);
				if (headIndex >= 0) active = false;
				else
				{
					headIndex = ~headIndex;
					if (headIndex == 0) active = false;
					else
					{
						var (itemTime, item) = MoveList.Model[headIndex - 1];
						if (item is not V1EMoveItem v1e) active = false;
						else
						{
							var previewPosition = times[dataMinTime];
							var y = (itemTime - track.TimeMin).Second * SpeedRate;
							var nextY = (dataMinTime - track.TimeMin).Second * SpeedRate;
							HeadNode.Init(v1e.Ease, new(v1e.Position, y), new(previewPosition, nextY));
						}
					}
				}

				HeadNode.gameObject.SetActive(active && Data.Count > 0);
			}
		}
	}
}