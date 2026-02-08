#nullable enable

using MusicGame.ChartEditor.TrackLine;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	[RequireComponent(typeof(PrefabHandler))]
	public class EditTrackMovementContent : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private bool isLrPos;

		[field: SerializeField]
		public Toggle Toggle1 { get; set; } = default!;

		[field: SerializeField]
		public Toggle Toggle2 { get; set; } = default!;

		[field: SerializeField]
		public Transform MovementContentRoot { get; set; } = default!;

		public bool IsShow1
		{
			get => isShow1;
			set
			{
				isShow1 = value;
				Toggle1.SetIsOnWithoutNotify(value);
				sideMovement1Handler?.gameObject.SetActive(value);
				Toggle2.SetIsOnWithoutNotify(!value);
				sideMovement2Handler?.gameObject.SetActive(!value);
			}
		}

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(Toggle1, isOn =>
			{
				if (!isOn) return;
				IsShow1 = true;
			}),
			new ToggleRegistrar(Toggle2, isOn =>
			{
				if (!isOn) return;
				IsShow1 = false;
			})
		};

		// Private
		private PrefabHandler handler = default!;
		private bool isShow1 = true;
		private PrefabHandler? sideMovement1Handler = default!;
		private PrefabHandler? sideMovement2Handler = default!;

		// Defined Functions
		[Inject]
		private void Construct()
		{
			handler = GetComponent<PrefabHandler>();
			handler.OnPluginAdded += OnPluginAdded;
		}

		// Event Handlers
		private void OnPluginAdded(string id)
		{
			if (id.EndsWith("1"))
			{
				sideMovement1Handler = handler.GetPlugin(id);
				sideMovement1Handler?.transform.SetParent(MovementContentRoot, false);
			}

			if (id.EndsWith("2"))
			{
				sideMovement2Handler = handler.GetPlugin(id);
				sideMovement2Handler?.transform.SetParent(MovementContentRoot, false);
			}

			IsShow1 = IsShow1;
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			if (isLrPos)
			{
				Toggle1.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedLeftColor;
				Toggle2.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedRightColor;
			}
			else
			{
				Toggle1.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedPosColor;
				Toggle2.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedWidthColor;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			handler.OnPluginAdded -= OnPluginAdded;
		}
	}
}