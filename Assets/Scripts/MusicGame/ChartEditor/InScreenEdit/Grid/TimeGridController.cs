using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Note.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class TimeGridController : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private float pixelPerGameYForTimingBlock;
		[SerializeField] private SpriteRenderer lineImage;
		[SerializeField] private TimingBlock timingBlock;
		[SerializeField] private RectTransform timingBlockTransform;
		[SerializeField] private RectTransform timingBlockRoot;

		public RectTransform TimingBlockRoot
		{
			get => timingBlockRoot;
			set => timingBlockRoot = value;
		}

		// Private
		private GameAudioPlayer music;
		private NotifiableProperty<ISpeed> speed;

		private GridTimeUI ui;
		private T3Time time;
		private V1LSMoveList moveList;

		// Static

		// Defined Functions
		[Inject]
		private void Construct(GameAudioPlayer music, NotifiableProperty<ISpeed> speed)
		{
			this.music = music;
			this.speed = speed;
		}

		public void Init(GridTimeUI ui, T3Time time, Color color)
		{
			this.ui = ui;
			this.time = time;
			moveList = new(time, 0);
			lineImage.color = color;
			timingBlock.Init(time, color);
			ui.OnBeforeResetGrid += Destroy;
		}

		public void Destroy()
		{
			transform.localPosition = new(0, 100);
			timingBlockTransform.anchoredPosition = new(0, 100 * pixelPerGameYForTimingBlock);
			ui.ReleaseTimeGrid(this);
			ui.OnBeforeResetGrid -= Destroy;
		}

		// System Functions
		void Start()
		{
			if (timingBlockRoot == null)
			{
				Debug.LogWarning("Timing block root is not set");
				timingBlockTransform.gameObject.SetActive(false);
			}
			else
			{
				timingBlockTransform.SetParent(TimingBlockRoot, false);
				timingBlockTransform.gameObject.SetActive(true);
			}
		}

		void Update()
		{
			T3Time chartTime = music.ChartTime;
			var y = -moveList.GetPos(chartTime) * speed.Value.SpeedRate;
			transform.localPosition = new(0, y);
			timingBlockTransform.anchoredPosition = new(0, y * pixelPerGameYForTimingBlock);
			if (chartTime > time) Destroy();
		}

		void OnEnable()
		{
			if (timingBlock != null) timingBlock.gameObject.SetActive(true);
		}

		void OnDisable()
		{
			if (timingBlock != null) timingBlock.gameObject.SetActive(false);
		}
	}
}