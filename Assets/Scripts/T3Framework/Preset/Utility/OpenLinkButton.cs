#nullable enable

using System.Diagnostics;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.Utility
{
	public class OpenLinkButton : T3MonoBehaviour
	{
		// Serializable and Public
		// TODO: Make it a general link open button with more types of links
		[Header("Network Url Only")] [SerializeField]
		private string link = string.Empty;

		[SerializeField] private Button button = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, OnButtonClick)
		};

		private void OnButtonClick()
		{
			try
			{
				Process.Start(link);
			}
			catch
			{
				UnityEngine.Debug.LogError($"Failed to open link: {link}");
			}
		}
	}
}