#nullable enable

using UnityEngine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawColorData
	{
		public float r { get; }

		public float g { get; }

		public float b { get; }

		public float a { get; }

		public RawColorData(Color color)
		{
			r = color.r;
			g = color.g;
			b = color.b;
			a = color.a;
		}
	}
}
