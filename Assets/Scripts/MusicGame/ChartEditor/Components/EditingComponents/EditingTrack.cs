using UnityEngine;
using static Takana3.MusicGame.Values;

public class EditingTrack : EditingComponent
{
    // Serializable and Public
    public Track Track => Component as Track;

    public override SelectTarget Type => SelectTarget.Track;

    public override bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            if (Track.Controller != null)
            {
                Track.Controller.IsHighlight = value;
            }
            _isSelected = value;
        }
    }

    public int Layer { get; set; } = 0;

    // Private
    private bool _isSelected = false;
    private bool isLayerEventAdd = false;

    // Static

    // Defined Functions
    public EditingTrack(Track track) : base(track) { }

    public override bool Instantiate()
    {
        if (!isLayerEventAdd)
        {
            EventManager.AddListener(EventManager.EventName.ChangeTrackLayer, OnSelectedLayerChanged);
            isLayerEventAdd = true;
        }
        if (TimeProvider.Instance.ChartTime > Track.TimeEnd + TimeAfterEnd) return true;
        else
        {
            Track.Instantiate();
            if (IsSelected) Track.Controller.IsHighlight = true;
            if (TrackLayerManager.Instance != null)
            {
                OnSelectedLayerChanged(TrackLayerManager.Instance.SelectedLayer);
            }
            return true;
        }
    }

    public void OnSelectedLayerChanged(object oLayer)
    {
        int layer = (int)oLayer;
        if (Track.Controller != null)
        {
            if (layer == 0 || Layer == 0 || layer == Layer) Track.Controller.IsHidden = false;
            else
            {
                Track.Controller.IsHidden = true;
                SelectManager.Instance.UnselectTrack(Id);
            }
        }
    }
}
