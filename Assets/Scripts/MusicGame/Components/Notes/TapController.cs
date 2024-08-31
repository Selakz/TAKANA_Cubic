using UnityEngine;
using static Takana3.MusicGame.Values;

public class TapController : BaseNoteController
{
    // Serializable and Public
    [SerializeField] private Highlight2D highlight;

    public override BaseNote Info => _tap;

    public override float GameWidth
    {
        // 虽然Note宽度比轨道小，但名义上认为是轨道宽度，即小的这部分完全内部封装。
        get => Info.BelongingTrack.Controller.GameWidth;
        protected set
        {
            float width = value > 2 * TapTrackGap ? value - TapTrackGap : value;
            sprite.size = new(Camera.main.G2WPosX(width), sprite.size.y);
            boxCollider.size = new(Camera.main.G2WPosX(width), boxCollider.size.y);
        }
    }
    public float GamePos
    {
        get => Camera.main.W2GPosY(transform.localPosition.y);
        set
        {
            transform.localPosition = new(0, Camera.main.G2WPosY(value));
        }
    }

    // Private
    private float Current => TimeProvider.Instance.ChartTime;

    private Tap _tap;
    private bool isAuto = false;
    private bool isJudged = false;
    private InputInfo inputInfo;
    private InputInfo realTimeInfo;

    // Static


    // Defined Functions

    public override void InfoInit(BaseNote note, InputInfo inputInfo)
    {
        if (note is Tap tap) _tap = tap;
        else Debug.LogError("Parameter should be Tap.");
        realTimeInfo = new() { Note = _tap };
        if (inputInfo != null)
        {
            this.inputInfo = inputInfo;
            isAuto = true;
        }
    }

    public override void SpriteInit()
    {
        sprite.size = new(Camera.main.G2WPosX(1.0f), 0.07f);
        sprite.transform.localScale = new(1, Camera.main.G2WPosY(3.5f));
        boxCollider.size = new(Camera.main.G2WPosX(1.0f), Camera.main.G2WPosY(0.5f));
    }

    public override bool HandleInput(float timeInput)
    {
        var judgeResult = Info.JudgeInfo.GetJudge(_tap.TimeJudge - timeInput);
        if (judgeResult != JudgeResult.NotHit)
        {
            realTimeInfo.TimeInput = timeInput;
            EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (judgeResult, realTimeInfo));
            sprite.gameObject.SetActive(false);
            // TODO: 不同样式的判定效果
            var effect = Instantiate(hitEffect);
            effect.transform.position = new(transform.position.x, _tap.BelongingTrack.BelongingLine.ThisObject.transform.position.y);
            _tap.BelongingTrack.Controller.LaneHitEffect(judgeResult);
            isJudged = true;
            return true;
        }
        return false;
    }

    public override void UpdatePos()
    {
        GameWidth = GameWidth;
        GamePos = _tap.GetY(Current);
        //float posY = Camera.main.G2WPosY(_tap.GetY(Current));
        //transform.localPosition = new(0, posY);
    }

    public void HandleInputInfo()
    {
        if (!isJudged && Current >= _tap.InputInfo.TimeInput) HandleInput(inputInfo.TimeInput);
    }

    public override void Highlight()
    {
        highlight.Highlight();
    }

    public override void Dehighlight()
    {
        highlight.Dehighlight();
    }

    // System Functions
    void Start()
    {
        SpriteInit();
        realTimeInfo = new() { Note = _tap };
    }

    void Update()
    {
        if (Current > _tap.TimeJudge + TimeAfterEnd)
        {
            Destroy(gameObject);
            return;
        }
        else if (Current > _tap.TimeJudge + _tap.JudgeInfo.TimeMiss)
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
