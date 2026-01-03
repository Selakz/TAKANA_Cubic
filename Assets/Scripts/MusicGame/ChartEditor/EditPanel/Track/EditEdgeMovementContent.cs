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
	public class EditEdgeMovementContent : T3MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Toggle LeftToggle { get; set; } = default!;

		[field: SerializeField]
		public Toggle RightToggle { get; set; } = default!;

		[field: SerializeField]
		public Transform MovementContentRoot { get; set; } = default!;

		public bool IsShowLeft
		{
			get => isShowLeft;
			set
			{
				isShowLeft = value;
				LeftToggle.SetIsOnWithoutNotify(value);
				leftMovementHandler?.gameObject.SetActive(value);
				RightToggle.SetIsOnWithoutNotify(!value);
				rightMovementHandler?.gameObject.SetActive(!value);
			}
		}

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(LeftToggle, isOn =>
			{
				if (!isOn) return;
				IsShowLeft = true;
			}),
			new ToggleRegistrar(RightToggle, isOn =>
			{
				if (!isOn) return;
				IsShowLeft = false;
			})
		};

		// Private
		private PrefabHandler handler = default!;
		private bool isShowLeft = true;
		private PrefabHandler? leftMovementHandler = default!;
		private PrefabHandler? rightMovementHandler = default!;

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
				leftMovementHandler = handler.GetPlugin(id);
				leftMovementHandler?.transform.SetParent(MovementContentRoot, false);
			}

			if (id.EndsWith("2"))
			{
				rightMovementHandler = handler.GetPlugin(id);
				rightMovementHandler?.transform.SetParent(MovementContentRoot, false);
			}

			IsShowLeft = IsShowLeft;
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			LeftToggle.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedLeftColor;
			RightToggle.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedRightColor;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			handler.OnPluginAdded -= OnPluginAdded;
		}
	}
}