#nullable enable

using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Render
{
	public sealed class VectorArrayConverter
	{
		private readonly Vector4[] data;

		public VectorArrayConverter(Vector4[] data)
		{
			this.data = data;
		}

		public VectorArrayConverter(int length)
		{
			data = new Vector4[(length + 1) / 2];
		}

		public int Length => data.Length * 2;

		public Vector2 this[int index]
		{
			get
			{
				var v = data[index >> 1];
				return (index & 1) == 0 ? new(v.x, v.y) : new(v.z, v.w);
			}
			set
			{
				int idx = index >> 1;
				var v = data[idx];
				if ((index & 1) == 0)
				{
					v.x = value.x;
					v.y = value.y;
				}
				else
				{
					v.z = value.x;
					v.w = value.y;
				}
				data[idx] = v;
			}
		}

		public Vector4[] AsVector4Array() => data;

		public float Interpolate(float x)
		{
			var points = this;
			if (x <= points[0].x) return points[0].y;
			if (x >= points[^1].x) return points[^1].y;

			int low = 0;
			int high = points.Length - 1;
			while (low <= high)
			{
				int mid = (low + high) / 2;
				if (points[mid].x < x)
					low = mid + 1;
				else
					high = mid - 1;
			}

			var pLeft = points[high];
			var pRight = points[low];
			float t = (x - pLeft.x) / (pRight.x - pLeft.x);
			return Mathf.Lerp(pLeft.y, pRight.y, t);
		}
	}
}
