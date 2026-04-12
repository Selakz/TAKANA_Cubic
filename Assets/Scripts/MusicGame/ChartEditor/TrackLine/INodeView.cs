#nullable enable

using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public interface INodeView
	{
		public Modifier<Color> ColorModifier { get; }

		public bool IsEditable { get; set; }
	}
}