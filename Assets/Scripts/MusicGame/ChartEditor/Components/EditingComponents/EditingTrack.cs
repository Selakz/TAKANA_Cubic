using UnityEngine;
using static Takana3.MusicGame.Values;

public class EditingTrack : EditingComponent
{
    // Serializable and Public
    public Track Track => Component as Track;

    public override SelectTarget Type => SelectTarget.Track;

    public int Layer { get; set; } = 0;

    public bool IsLineShown { get; private set; }

    // Private

    // Static

    // Defined Functions
    public EditingTrack(Track track) : base(track) { }

    public void ShowLine()
    {
        if (IsLineShown) return;
        IsLineShown = true;
        TrackLineManager.Instance.Decorate(Track);
        Debug.Log($"track {Id} shows line.");
    }

    public void HideLine()
    {
        if (!IsLineShown) return;
        IsLineShown = false;
        Debug.Log($"track {Id} hides line.");
    }

    public override bool Instantiate()
    {
        if (TimeProvider.Instance.ChartTime > Track.TimeEnd + TimeAfterEnd) return true;
        else
        {
            Track.Instantiate();
            if (IsSelected) Track.Controller.Highlight();
            return true;
        }
    }

    public override void Select()
    {
        if (IsSelected) return;
        IsSelected = true;
        if (Track.Controller != null) Track.Controller.Highlight();
        Debug.Log($"track {Id} selected.");
    }

    public override void Unselect()
    {
        if (!IsSelected) return;
        IsSelected = false;
        if (Track.Controller != null) Track.Controller.Dehighlight();
        HideLine();
        Debug.Log($"track {Id} unselected.");
    }
}
