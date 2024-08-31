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
    public bool IsSelected { get; protected set; }

    // Private

    // Static

    // Defined Functions
    public EditingComponent(IComponent component)
    {
        Component = component;
    }

    public void Initialize(MusicSetting setting)
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

    /// <summary> һ������½���SelectManager���� </summary>
    public abstract void Select();

    /// <summary> һ������½���SelectManager���� </summary>
    public abstract void Unselect();

    public abstract bool Instantiate();
}
