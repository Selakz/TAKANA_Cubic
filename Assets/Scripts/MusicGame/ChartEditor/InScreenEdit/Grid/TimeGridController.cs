using MusicGame.Components.Movement;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using UnityEngine;

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

		// Private
		private GridTimeRetriever timeRetriever;
		private T3Time time;
		private V1LSMoveList moveList;

		// Static

		// Defined Functions
		public void Init(GridTimeRetriever timeRetriever, T3Time time, Color color)
		{
			this.timeRetriever = timeRetriever;
			this.time = time;
			moveList = new(time, 0);
			lineImage.color = color;
			timingBlock.Init(time, color);
			timeRetriever.OnBeforeResetGrid += Destroy;
		}

		public void Destroy()
		{
			transform.localPosition = new(0, 100);
			timingBlockTransform.anchoredPosition = new(0, 100 * pixelPerGameYForTimingBlock);
			timeRetriever.ReleaseTimeGrid(this);
			timeRetriever.OnBeforeResetGrid -= Destroy;
		}

		// System Functions
		void Start()
		{
			timingBlockRoot = (RectTransform)GameObject.Find("/EditorCanvas/TimingBlocks").transform;
			timingBlockTransform.SetParent(timingBlockRoot, false);
		}

		void Update()
		{
			T3Time chartTime = LevelManager.Instance.Music.ChartTime;
			var y = -moveList.GetPos(chartTime) * LevelManager.Instance.LevelSpeed.SpeedRate;
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