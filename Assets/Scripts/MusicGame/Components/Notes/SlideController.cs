using UnityEngine;
using static Takana3.MusicGame.Values;

public class SlideController : BaseNoteController
{
	// Serializable and Public
	[SerializeField] private Highlight2D highlight;

	public override BaseNote Info => _slide;

	public override float GameWidth
	{
		// 虽然Note宽度比轨道小，但名义上认为是轨道宽度，即小的这部分完全内部封装。
		get => Info.BelongingTrack.Controller.GameWidth;
		protected set
		{
			float width = value > 2 * SlideTrackGap ? value - SlideTrackGap : value;
			sprite.size = new(Camera.main.G2WPosX(width), sprite.size.y);
			boxCollider.size = new(Camera.main.G2WPosX(width), boxCollider.size.y);
		}
	}

	public float GamePos
	{
		get => Camera.main.W2GPosY(transform.localPosition.y);
		set { transform.localPosition = new(0, Camera.main.G2WPosY(value)); }
	}

	public override bool IsHighlight
	{
		get => _isHighlight;
		set
		{
			highlight.IsHighlight = value;
			_isHighlight = value;
		}
	}

	// Private
	private float Current => TimeProvider.Instance.ChartTime;

	private Slide _slide;
	private bool isAuto = false;
	private bool isJudged = false;
	private InputInfo inputInfo;
	private InputInfo realTimeInfo;
	private bool _isHighlight = false;

	// Static

	// Defined Functions
	public override void InfoInit(BaseNote note, InputInfo inputInfo)
	{
		if (note is Slide slide) _slide = slide;
		else Debug.LogError("Parameter should be Slide.");
		realTimeInfo = new() { Note = _slide };
		if (inputInfo != null)
		{
			this.inputInfo = inputInfo;
			isAuto = true;
		}
	}

	public override void SpriteInit()
	{
		sprite.size = new(Camera.main.G2WPosX(1.0f), 0.1f);
		sprite.transform.localScale = new(1, Camera.main.G2WPosY(3f));
		boxCollider.size = new(Camera.main.G2WPosX(1.0f), Camera.main.G2WPosY(0.5f));
	}

	public override bool HandleInput(float timeInput)
	{
		var judgeResult = Info.JudgeInfo.GetJudge(_slide.TimeJudge - timeInput);
		if (judgeResult != JudgeResult.NotHit)
		{
			realTimeInfo.TimeInput = timeInput;
			EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (judgeResult, realTimeInfo));
			EventManager.Trigger(EventManager.EventName.PlayHitSound, 0.75f);
			sprite.gameObject.SetActive(false);
			// TODO: 不同样式的判定效果
			var effect = Instantiate(hitEffect);
			effect.transform.position = new(transform.position.x,
				_slide.BelongingTrack.BelongingLine.ThisObject.transform.position.y);
			_slide.BelongingTrack.Controller.LaneHitEffect(judgeResult);
			isJudged = true;
			return true;
		}

		return false;
	}

	public override void UpdatePos()
	{
		GameWidth = GameWidth;
		GamePos = _slide.GetY(Current);
	}

	public void HandleInputInfo()
	{
		if (!isJudged && Current >= _slide.InputInfo.TimeInput) HandleInput(inputInfo.TimeInput);
	}

	// System Functions
	void Start()
	{
		SpriteInit();
		realTimeInfo = new() { Note = _slide };
	}

	void Update()
	{
		if (Current > _slide.TimeJudge + TimeAfterEnd)
		{
			Destroy(gameObject);
			return;
		}
		else if (Current > _slide.TimeJudge + _slide.JudgeInfo.TimeMiss)
		{
			if (!isJudged)
			{
				EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.LateMiss, realTimeInfo));
				isJudged = true;
				sprite.gameObject.SetActive(false);
				boxCollider.enabled = false;
				return;
			}
		}

		if (!isJudged) UpdatePos();

		if (isAuto) HandleInputInfo();
	}
}