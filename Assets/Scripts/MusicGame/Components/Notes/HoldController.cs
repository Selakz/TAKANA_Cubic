using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using static Takana3.MusicGame.Values;

// Hold��ScalePart����������Hold�������ͼ����ScalePartͳһ�޸�scale���������ͼ�����Ķ���
public class HoldController : BaseNoteController
{
    // Serializable and Public
    [SerializeField] private Highlight2D holdHighlight;
    [SerializeField] private Highlight2D holdStartHighlight;
    [SerializeField] private SpriteRenderer holdStartSprite;

    public override BaseNote Info => _hold;
    public override float GameWidth
    {
        // ��ȻNote��ȱȹ��С������������Ϊ�ǹ����ȣ���С���ⲿ����ȫ�ڲ���װ��
        get => Info.BelongingTrack.Controller.GameWidth;
        protected set
        {
            float width = value > 2 * TapTrackGap ? value - TapTrackGap : value;
            sprite.size = new(Camera.main.G2WPosX(width), sprite.size.y);
            holdStartSprite.size = new(Camera.main.G2WPosX(width), holdStartSprite.size.y);
            boxCollider.size = sprite.size;
        }
    }
    public float GameLength
    {
        get => Camera.main.W2GPosY(sprite.size.y);
        set
        {
            sprite.size = new(sprite.size.x, Camera.main.G2WPosY(value));
            sprite.transform.localPosition = defaultPosition * new Vector2(1, value);
            boxCollider.size = new(sprite.size.x, Mathf.Max(sprite.size.y, Camera.main.G2WPosY(0.5f)));
            boxCollider.offset = sprite.transform.localPosition;
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

    private Hold _hold;
    private bool isPaused = false;
    private bool isAuto = false;
    private bool isJudged = false;
    private bool isFailed = false;
    private bool isReleased = false;
    private JudgeResult resultOnHit; // ��¼����ʱ���ж�����һ���
    private float lastTimeRelease = 0f;
    private HoldInputInfo inputInfo;
    private HoldInputInfo realTimeInfo;

    // Static?
    Vector2 defaultPosition;

    // Defined Functions

    public override void InfoInit(BaseNote note, InputInfo inputInfo)
    {
        if (note is Hold hold) _hold = hold;
        else Debug.LogError("Parameter should be Hold.");
        realTimeInfo = new() { Note = _hold };
        if (inputInfo != null)
        {
            if (inputInfo is HoldInputInfo info)
            {
                this.inputInfo = info;
                isAuto = true;
            }
            else Debug.LogError("Parameter should be HoldInputInfo.");
        }
    }

    public override void SpriteInit()
    {
        defaultPosition = new(0, Camera.main.G2WPosY(0.5f));
        holdStartSprite.size = new(Camera.main.G2WPosX(1.0f), 0.1f);
        holdStartSprite.transform.localScale = new(1, Camera.main.G2WPosY(3f));
        holdStartSprite.transform.localPosition = new(0, 0);
    }

    public override bool HandleInput(float timeInput)
    {
        var judgeResult = _hold.JudgeInfo.GetJudge(_hold.TimeJudge - timeInput);
        if (judgeResult != JudgeResult.NotHit)
        {
            resultOnHit = judgeResult;
            isJudged = true;

            if (!isAuto) realTimeInfo.TimeInput = timeInput;
            // TODO: �޸�Hold����ж����߼�
            EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (judgeResult, (InputInfo)realTimeInfo));
            _hold.BelongingTrack.Controller.LaneHitEffect(judgeResult);
            return true;
        }
        return false;
    }

    public override void UpdatePos()
    {
        GameWidth = GameWidth; // �������ϵĿ������ʵ�ʿ��...�����д��
        if (Current < _hold.TimeJudge && !isJudged)
        {
            GamePos = _hold.GetY(Current);
            GameLength = _hold.GetScale(Current);
            //float posY = Camera.main.G2WPosY(_hold.GetY(Current));
            //transform.localPosition = new(0, posY);
            //scalePart.localScale = defaultScale * new Vector2(1, _hold.GetScale(Current));
            //scalePart.localPosition = defaultPosition * new Vector2(1, _hold.GetScale(Current));
        }
        else if (Current < _hold.TimeJudge && isJudged)
        {
            GamePos = 0f;
            GameLength = _hold.GetScale(Current) + _hold.GetY(Current);
            //transform.localPosition = new(0, 0);
            //scalePart.localScale = defaultScale * new Vector2(1, _hold.GetScale(Current) + _hold.GetY(Current));
            //scalePart.localPosition = defaultPosition * new Vector2(1, _hold.GetScale(Current) + _hold.GetY(Current));
        }
        else if (Current >= _hold.TimeJudge && Current < _hold.TimeEnd)
        {
            GamePos = 0f;
            GameLength = _hold.GetScale(Current);
            //transform.localPosition = new(0, 0);
            //scalePart.localScale = defaultScale * new Vector2(1, _hold.GetScale(Current));
            //scalePart.localPosition = defaultPosition * new Vector2(1, _hold.GetScale(Current));
        }
    }

    public void DetectHold(InputAction.CallbackContext context)
    {
        if (!isAuto)
        {
            if (context.interaction is PressInteraction)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Performed:
                        Continue();
                        break;
                    case InputActionPhase.Canceled:
                        Release();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void HandleFail()
    {
        if (!isFailed)
        {
            // TODO: û�㵽�͵㵽���ɿ������������������takumiû��holdinterval���ֶ����ˣ�
            // ��ΪʹHold��ҵ�����
            if ((isJudged && isReleased && Current > lastTimeRelease + TimeHoldInterval)
            || (!isJudged && Current > _hold.TimeJudge + _hold.JudgeInfo.TimeMiss))
            {
                isFailed = true; isJudged = false;
                realTimeInfo.ReleaseTimes.Add((lastTimeRelease, Current));
                EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.LateMiss, (InputInfo)realTimeInfo));
                Debug.Log("hold" + _hold.Id + " failed.");

                base.sprite.gameObject.SetActive(false);
            }
        }
    }

    public void Continue()
    {
        if (!isFailed && isJudged && isReleased)
        {
            isReleased = false;
            realTimeInfo.ReleaseTimes.Add((lastTimeRelease, Current));
            Debug.Log($"Hold {_hold.Id} continued!");

            if (!isPaused)
            {
                base.sprite.gameObject.SetActive(true);
            }
        }
    }

    public void Release()
    {
        if (!isFailed && isJudged && !isReleased)
        {
            isReleased = true;
            lastTimeRelease = Current;
            Debug.Log($"Hold {_hold.Id} released!");

            if (!isPaused)
            {
                base.sprite.gameObject.SetActive(false);
            }
        }
    }

    public void HandleInputInfo() // ģ������
    {
        if (!isJudged && Current > inputInfo.TimeInput) HandleInput(inputInfo.TimeInput);
        if (inputInfo.IsRelease(Current)) Release();
        else Continue();
    }

    public override void Highlight()
    {
        holdHighlight.Highlight();
        holdStartHighlight.Highlight();
    }

    public override void Dehighlight()
    {
        holdHighlight.Dehighlight();
        holdStartHighlight.Dehighlight();
    }

    // System Functions
    void Start()
    {
        SpriteInit();
        realTimeInfo = new() { Note = _hold, ReleaseTimes = new() };
        EventManager.AddListener(EventManager.EventName.Pause, () => isPaused = true);
        EventManager.AddListener(EventManager.EventName.Resume, () => isPaused = false);
    }

    void Update()
    {
        if (!isPaused)
        {
            if (Current > _hold.TimeEnd + TimeAfterEnd)
            {
                Destroy(gameObject);
                return;
            }
            else if (Current > _hold.TimeEnd)
            {
                if (isJudged && !isFailed)
                {
                    EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.CriticalPerfect, (InputInfo)realTimeInfo));
                    var effect = Instantiate(hitEffect);
                    effect.transform.position = effect.transform.position = new(transform.position.x, _hold.BelongingTrack.BelongingLine.ThisObject.transform.position.y);
                    isJudged = false;
                }
                base.sprite.gameObject.SetActive(false);
                boxCollider.enabled = false;
                return;
            }
            else if (Current > _hold.TimeJudge)
            {
                holdStartSprite.gameObject.SetActive(false);
            }


            HandleFail();

            if (isAuto) HandleInputInfo();
        }

        UpdatePos();
    }
}
