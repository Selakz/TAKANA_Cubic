#nullable enable

using System;
using T3Framework.Runtime.Modifier;
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
				if (enabledModifier is not null) return enabledModifier;
				var defaultEnabled = collider.enabled;
				enabledModifier = new Modifier<bool>(
					() => collider.enabled,
					value => collider.enabled = value,
					_ => defaultEnabled);
				return enabledModifier;
			}
		}

		public Modifier<Vector3> CenterModifier
		{
			get
			{
				if (centerModifier is not null) return centerModifier;
				var defaultCenter = collider.center;
				centerModifier = new Modifier<Vector3>(
					() => collider.center,
					value => collider.center = value,
					_ => defaultCenter);
				return centerModifier;
			}
		}

		public Modifier<Vector3> SizeModifier
		{
			get
			{
				if (sizeModifier is not null) return sizeModifier;
				var defaultSize = collider.size;
				sizeModifier = new Modifier<Vector3>(
					() => collider.size,
					value => collider.size = value,
					_ => defaultSize);
				return sizeModifier;
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