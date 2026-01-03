#nullable enable

using System;
using System.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	[RequireComponent(typeof(PrefabHandler))]
	public class EditMoveListContent : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private LayoutElement layoutElement = default!;
		[SerializeField] private Transform listItemRoot = default!;

		public PrefabHandler Handler { get; private set; } = default!;

		public Modifier<float> HeightModifier => heightModifier ??=
			new(() => layoutElement.minHeight, value => layoutElement.minHeight = value);

		// Private
		private Modifier<float>? heightModifier;

		// Constructor
		[Inject]
		private void Construct()
		{
			Handler = GetComponent<PrefabHandler>();
			Handler.OnPluginAdded += OnPluginAdded;
		}

		public void SortListItem(Comparison<PrefabHandler> comparison)
		{
			var plugins = Handler.GetPlugins().Where(plugin => plugin.transform.parent == listItemRoot).ToArray();
			Array.Sort(plugins, comparison);
			for (int i = 0; i < plugins.Length; i++)
			{
				plugins[i].transform.SetSiblingIndex(i);
			}
		}

		// Event Handlers
		private void OnPluginAdded(string id)
		{
			if (id.StartsWith("ListItem"))
			{
				var plugin = Handler.GetPlugin(id);
				plugin?.transform.SetParent(listItemRoot, false);
			}
		}

		// System Functions
		void OnDestroy()
		{
			Handler.OnPluginAdded -= OnPluginAdded;
		}
	}

	public class MoveListContentRegistrar : IEventRegistrar
	{
		private readonly IEventRegistrar[] registrars;

		public MoveListContentRegistrar(EditMoveListContent content)
		{
			registrars = new IEventRegistrar[]
			{
				CustomRegistrar.Generic<Action<string>>(
					a => content.Handler.OnPluginAdded += a,
					a => content.Handler.OnPluginAdded -= a,
					_ => content.SortListItem(ListItemCompare))
			};
		}

		public void Register()
		{
			foreach (var registrar in registrars) registrar.Register();
		}

		public void Unregister()
		{
			foreach (var registrar in registrars) registrar.Unregister();
		}

		private static int ListItemCompare(PrefabHandler a, PrefabHandler b)
		{
			var nodeA = a.Script<EditV1EItemContent>();
			var nodeB = b.Script<EditV1EItemContent>();
			var timeA = int.TryParse(nodeA.TimeInputField.text, out var time1) ? time1 : 0;
			var timeB = int.TryParse(nodeB.TimeInputField.text, out var time2) ? time2 : 0;
			return timeA.CompareTo(timeB);
		}
	}
}