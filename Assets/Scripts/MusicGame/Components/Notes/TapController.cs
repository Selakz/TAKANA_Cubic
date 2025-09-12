using MusicGame.Components.Chart;
using MusicGame.Gameplay;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;

namespace MusicGame.Components.Notes
{
	public class TapController : BaseNoteController<Tap>
	{
		// Serializable and Public

		// Private
		private T3Time Current => LevelManager.Instance.Music.ChartTime;

		// Static

		// Defined Functions
		public override void Destroy()
		{
			// Released to pool
			Model.Controller = null;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(LevelManager.Instance.PoolingStorage);
		}

		// Event Handlers
		private void LevelOnReset(T3Time chartTime)
		{
			if (chartTime < Model.TimeInstantiate ||
			    chartTime > Model.TimeJudge + ISingletonSetting<PlayfieldSetting>.Instance.TimeAfterEnd)
			{
				Model.Destroy();
			}
		}

		public void ChartOnUpdate(ChartInfo chartInfo)
		{
			if (!chartInfo.Contains(Model.Id))
			{
				Model.Destroy();
			}
		}

		// System Functions
		void OnEnable()
		{
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);

			PositionModifier.Register(
				value => new(value.x, Model.Movement.GetPos(Current) * LevelManager.Instance.LevelSpeed.SpeedRate),
				0);
			ScaleModifier.Register(
				_ =>
				{
					var width = Model.Parent.Width;
					var tapTrackGap = ISingletonSetting<PlayfieldSetting>.Instance.TrackGap1;
					return new(width > 2 * tapTrackGap ? width - tapTrackGap : width, 0);
				},
				0);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);

			PositionModifier.Unregister(0);
			ScaleModifier.Unregister(0);
		}

		protected override void Update()
		{
			base.Update();

			if (Current > Model.TimeJudge + ISingletonSetting<PlayfieldSetting>.Instance.TimeAfterEnd)
			{
				Model.Destroy();
			}
		}
	}
}