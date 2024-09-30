using Takana3.Settings;
using UnityEngine;
using static Takana3.MusicGame.Values;

public abstract class EditingComponent : IComponent
{
    public IComponent Component { get; private set; }

    // Implement IComponent
    public int Id => Component.Id;
    public bool IsInitialized => Component.IsInitialized;
    public GameObject ThisObject => Component.ThisObject;
    public float TimeInstantiate => Component.TimeInstantiate;

    // Self Properties
    public abstract SelectTarget Type { get; }
    public abstract bool IsSelected { get; set; }

    // Private

    // Static

    // Defined Functions
    public EditingComponent(IComponent component)
    {
        Component = component;
    }

    public virtual void Initialize(MusicSetting setting)
    {
        Component.Initialize(setting);
    }

    public static EditingComponent AutoWrapByType(IComponent component)
    {
        return component switch
        {
            Track track => new EditingTrack(track),
            Tap tap => new EditingTap(tap),
            Slide slide => new EditingSlide(slide),
            Hold hold => new EditingHold(hold),
            JudgeLine judgeLine => new EditingJudgeLine(judgeLine),
            _ => null,
        };
    }

    public abstract bool Instantiate();
}
