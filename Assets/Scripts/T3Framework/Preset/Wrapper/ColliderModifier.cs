#nullable enable

using System;
using T3Framework.Runtime;
using UnityEngine;

namespace T3Framework.Preset.Wrapper
{
	public interface IColliderModifier
	{
		Modifier<bool> EnabledModifier { get; }
	}

	[Serializable]
	public class BoxColliderModifier : IColliderModifier
	{
		[SerializeField] private BoxCollider collider;

		public BoxCollider Value => collider;

		public Modifier<bool> EnabledModifier
		{
			get
			{
				var defaultEnabled = collider.enabled;
				return enabledModifier ??= new Modifier<bool>(
					() => collider.enabled,
					value => collider.enabled = value,
					_ => defaultEnabled);
			}
		}

		public Modifier<Vector3> CenterModifier
		{
			get
			{
				var defaultCenter = collider.center;
				return centerModifier ??= new Modifier<Vector3>(
					() => collider.center,
					value => collider.center = value,
					_ => defaultCenter);
			}
		}

		public Modifier<Vector3> SizeModifier
		{
			get
			{
				var defaultSize = collider.size;
				return sizeModifier ??= new Modifier<Vector3>(
					() => collider.size,
					value => collider.size = value,
					_ => defaultSize);
			}
		}

		public BoxColliderModifier(BoxCollider collider)
		{
			this.collider = collider;
		}

		private Modifier<bool>? enabledModifier;

		private Modifier<Vector3>? centerModifier;

		private Modifier<Vector3>? sizeModifier;
	}
}