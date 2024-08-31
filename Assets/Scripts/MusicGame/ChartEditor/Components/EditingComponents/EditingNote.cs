using UnityEngine;

public abstract class EditingNote : EditingComponent
{
    // Serializable and Public
    public BaseNote Note => Component as BaseNote;

    public override SelectTarget Type => SelectTarget.Note;

    // Private

    // Static

    // Defined Functions
    public EditingNote(BaseNote baseNote) : base(baseNote) { }

    public override abstract bool Instantiate();

    public override void Select()
    {
        if (IsSelected) return;
        IsSelected = true;
        if (Note.Controller != null) Note.Controller.Highlight();
    }

    public override void Unselect()
    {
        if (!IsSelected) return;
        IsSelected = false;
        if (Note.Controller != null) Note.Controller.Dehighlight();
    }
}
