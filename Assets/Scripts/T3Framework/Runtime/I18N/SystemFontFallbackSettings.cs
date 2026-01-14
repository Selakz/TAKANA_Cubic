// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	[CreateAssetMenu(fileName = "SystemFontFallbackSettings", menuName = "T3Framework/SystemFontFallbackSettings")]
	public class SystemFontFallbackSettings : ScriptableObject
	{
		[field: SerializeField]
		public PerLanguageSettings[] Settings { get; private set; } = null!;

		[Serializable]
		public struct PerLanguageSettings
		{
			public Language languageCode;
			public string[] fontFileNames;
		}
	}
}