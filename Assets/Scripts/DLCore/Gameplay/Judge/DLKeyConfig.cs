#nullable enable

using System.Collections.Generic;
using DLCore.Models;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DLCore.Gameplay.Judge
{
	[CreateAssetMenu(fileName = "DLKeyConfig", menuName = "DLGameplayConfig/DLKeyConfig")]
	public class DLKeyConfig : ScriptableObject
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<ColorVariant, Key[]> colorKeys = new();

		// Private
		private Dictionary<Key, ColorVariant>? keyColorMap;

		// Defined Functions
		public ColorVariant GetColor(Key key)
		{
			keyColorMap ??= BuildKeyColorMap();
			return keyColorMap.GetValueOrDefault(key, ColorVariant.Gray);
		}

		public static bool CanJudge(ColorVariant keyColor, ColorVariant comboColor) => keyColor switch
		{
			ColorVariant.Gray => comboColor == ColorVariant.Gray,
			ColorVariant.Dark => comboColor == ColorVariant.Dark,
			ColorVariant.Blue => comboColor is ColorVariant.Blue or ColorVariant.Purple,
			ColorVariant.Red => comboColor is ColorVariant.Red or ColorVariant.Purple,
			ColorVariant.Purple => comboColor is ColorVariant.Blue or ColorVariant.Red or ColorVariant.Purple,
			_ => false
		};

		private Dictionary<Key, ColorVariant> BuildKeyColorMap()
		{
			Dictionary<Key, ColorVariant> map = new();
			foreach (var (color, keys) in colorKeys.Value)
			{
				foreach (var key in keys) map[key] = color;
			}

			return map;
		}
	}
}