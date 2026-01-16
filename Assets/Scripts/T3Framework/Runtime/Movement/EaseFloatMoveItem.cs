#nullable enable

using System;
using DG.Tweening;
using UnityEngine;

namespace T3Framework.Runtime.Movement
{
	[Serializable]
	public struct EaseFloatMoveItem
	{
		[field: SerializeField]
		public float Duration { get; set; }

		[field: SerializeField]
		public float Position { get; set; }

		[field: SerializeField]
		public Ease Ease { get; set; }
	}
}