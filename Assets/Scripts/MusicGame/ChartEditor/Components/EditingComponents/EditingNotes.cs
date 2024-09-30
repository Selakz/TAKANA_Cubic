using Takana3.Settings;

public class EditingTap : EditingNote
{
    public EditingTap(Tap tap) : base(tap) { }

    public override bool Instantiate()
    {
        if (TimeProvider.Instance.ChartTime < Note.TimeJudge) Note.Instantiate();
        else
        {
            switch (EditingLevelManager.Instance.MusicSetting.Mode)
            {
                case GameMode.Common:
                    if (TimeProvider.Instance.ChartTime < Note.TimeJudge + Note.JudgeInfo.TimeMiss)
                    {
                        Note.Instantiate();
                        break;
                    }
                    else
                    {
                        EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.LateMiss, Note.InputInfo));
                        return true;
                    }
                case GameMode.Auto or _:
                    EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.CriticalPerfect, Note.InputInfo));
                    return true;
            }
        }
        Note.Controller.IsHighlight = IsSelected;
        return true;
    }
}

public class EditingSlide : EditingNote
{
    public EditingSlide(Slide tap) : base(tap) { }

    public override bool Instantiate()
    {
        if (TimeProvider.Instance.ChartTime < Note.TimeJudge) Note.Instantiate();
        else
        {
            switch (EditingLevelManager.Instance.MusicSetting.Mode)
            {
                case GameMode.Common:
                    if (TimeProvider.Instance.ChartTime < Note.TimeJudge + Note.JudgeInfo.TimeMiss)
                    {
                        Note.Instantiate();
                        break;
                    }
                    else
                    {
                        EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.LateMiss, Note.InputInfo));
                        return true;
                    }
                case GameMode.Auto or _:
                    EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.CriticalPerfect, Note.InputInfo));
                    return true;
            }
        }
        Note.Controller.IsHighlight = IsSelected;
        return true;
    }
}

public class EditingHold : EditingNote
{
    public Hold Hold => Note as Hold;

    public EditingHold(Hold hold) : base(hold) { }

    public override bool Instantiate()
    {
        if (TimeProvider.Instance.ChartTime < Note.TimeJudge) Note.Instantiate();
        else
        {
            switch (EditingLevelManager.Instance.MusicSetting.Mode)
            {
                case GameMode.Common:
                    if (TimeProvider.Instance.ChartTime < Note.TimeJudge + Note.JudgeInfo.TimeMiss)
                    {
                        Note.Instantiate();
                        break;
                    }
                    else
                    {
                        EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.LateMiss, Note.InputInfo));
                        EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.LateMiss, Note.InputInfo));
                        return true;
                    }
                case GameMode.Auto or _:
                    if (TimeProvider.Instance.ChartTime < Hold.TimeEnd)
                    {
                        Note.Instantiate();
                        Note.Controller.HandleInput(Note.InputInfo.TimeInput);
                        break;
                    }
                    else
                    {
                        EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.CriticalPerfect, Note.InputInfo));
                        EventManager.Trigger(EventManager.EventName.AddJudgeAndInputInfo, (JudgeResult.CriticalPerfect, Note.InputInfo));
                        return true;
                    }
            }
        }
        Note.Controller.IsHighlight = IsSelected;
        return true;
    }
}
