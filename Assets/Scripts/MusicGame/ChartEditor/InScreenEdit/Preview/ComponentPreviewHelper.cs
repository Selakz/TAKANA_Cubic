#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime;

// ReSharper disable RedundantJumpStatement
namespace MusicGame.ChartEditor.InScreenEdit.Preview
{
	public class ComponentPreviewHelper : IDisposable
	{
		private bool isShow;
		private ChartComponent? parent;

		public bool IsLegal => Component.IsWithinRange(Parent, 0);

		public bool IsShow
		{
			get => isShow;
			set
			{
				if (value == isShow) return;
				isShow = value;
				Update(0);
			}
		}

		public ChartComponent Component { get; private set; }

		public ChartComponent? Parent
		{
			get => parent;
			set
			{
				parent = value;
				if (parent is null)
				{
					Component.BelongingChart = null;
					return;
				}

				if (Component.IsWithinRange(value, 0)) Component.SetParent(value);
				else Component.BelongingChart = null;
			}
		}

		public ComponentPreviewHelper(IChartModel previewModel, ChartComponent parent)
		{
			isShow = true;
			this.parent = parent;
			Component = new ChartComponent(previewModel);
			if (IsLegal) Component.SetParent(parent);
		}

		public ComponentPreviewHelper(ChartComponent previewComponent)
		{
			Component = previewComponent;
			isShow = Component.BelongingChart is not null;
			parent = Component.Parent;
		}

		public void Update(T3Time distance)
		{
			Component.Model.Nudge(distance); // Not notify
			// Permutation: IsShow / IsInChart / IsLegal
			bool isInChart = Component.BelongingChart is not null || Component.Parent is not null;
			bool isLegal = Component.IsWithinRange(Parent, 0); // Already nudged
			if (IsShow)
			{
				if (isInChart && isLegal) Component.UpdateNotify();
				else if (isInChart && !isLegal)
				{
					Component.BelongingChart = null;
					Component.Parent = null;
				}
				else if (!isInChart && isLegal) Component.Parent = Parent;
				else if (!isInChart && !isLegal) return;
			}
			else
			{
				if (isInChart && isLegal)
				{
					Component.BelongingChart = null;
					Component.Parent = null;
				}
				else if (isInChart && !isLegal)
				{
					Component.BelongingChart = null;
					Component.Parent = null;
				}
				else if (!isInChart && isLegal) return;
				else if (!isInChart && !isLegal) return;
			}
		}

		public void Dispose() => IsShow = false;
	}
}