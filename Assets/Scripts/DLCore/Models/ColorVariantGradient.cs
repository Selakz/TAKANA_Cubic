#nullable enable

namespace DLCore.Models
{
	/// <summary>
	/// The color at a moment which may be transitioning from one variant to another.
	/// If fully settled as a color, <see cref="FromColor"/> equals <see cref="ToColor"/> and <see cref="Gradient"/> is meaningless.
	/// </summary>
	public struct ColorVariantGradient
	{
		public ColorVariant FromColor { get; set; }

		public ColorVariant ToColor { get; set; }

		/// <summary> Range [0, 1]. </summary>
		public float Gradient { get; set; }

		public ColorVariantGradient(ColorVariant color) : this()
		{
			FromColor = color;
			ToColor = color;
		}
	}
}
